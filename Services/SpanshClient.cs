using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ODEliteTracker.Helpers;
using ODEliteTracker.Models.Spansh;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Web;

namespace ODEliteTracker.Services
{
    public sealed class SpanshClient(HttpClient httpClient)
    {
        private readonly HttpClient spanshClient = httpClient;
        // args for exo route ?avoid_thargoids=1&from=Sol&loop=1&max_distance=50000&max_results=100&min_value=10000000&radius=150&range=70&use_mapping_value=1
        //r2r & world type
        private const string RICHES_ROUTE = "riches/route";
        private const string FLEETCARRIER_ROUTE = "fleetcarrier/route";
        private const string ROUTE = "route";
        private const string GALAXY_ROUTE = "generic/route";
        private const string TOURIST_ROUTE = "tourist/route";        
        private const string EXOBIOLOGY_ROUTE = "exobiology/route";
        private const string COLONISATION_ROUTE = "colonisation/route";
        private const string TRADE_ROUTE = "trade/route";
        private const string RESULTS = "results/"; // job guid after.  Returns the status of the job and the results if completed
        private const string SYSTEMS = "systems";
        private const string STATIONS_NAME = "stations";
        private const string SYSTEM = "system";
        private const string SEARCH_SYSTEMS = "search/systems"; // / system name as parameter.  Returns a list of systems containing the string
        private const string DUMP = "dump/"; // /systemaddress after.  Returns the spansh dump of the given id64 if it exists in spans databse

        internal EventHandler<HttpErrorRespone>? OnError;

        private readonly JsonSerializer ignoreNull = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
        };
        public async Task<SpanshSystemApiDump?> GetSystem(ulong address)
        {
            if (address == 0)
                return null;

            var ret = await spanshClient.GetFromJsonAsync<SpanshSystemApiDump>($"{DUMP}{address}");

            return ret;
        }

        public async Task<bool> CheckSystemNameKnown(string name)
        {
            string query = "?q=" + HttpUtility.UrlEncode(name);

            var response = await spanshClient.GetFromJsonAsync<SpanshSystemNameSearchResponse>($"{SEARCH_SYSTEMS}{query}");

            if (response == null)
                return false;

            foreach (var item in response.Results)
            {
                if (string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<SpanshSystemNameSearchResponse?> GetSystemNames(string name)
        {
            string query = "?q=" + HttpUtility.UrlEncode(name);

            var response = await spanshClient.GetFromJsonAsync<SpanshSystemNameSearchResponse>($"{SEARCH_SYSTEMS}{query}");

            if (response == null)
                return null;

            return response;
        }

        public async Task<SpanshResponseBase?> GetApiRoute(SpanshApiParameters parameters, Action<string>? textUpdate)
        {
            try
            {
                var maxTime = 60;
                //We have a job id, check for updates
                if (!string.IsNullOrEmpty(parameters.JobId))
                {
                    switch (parameters.GetRouteType())
                    {
                        case SpanshRouteType.None:
                            return null;
                        case SpanshRouteType.GalaxyPlotter:
                        case SpanshRouteType.GalaxyPlotterOld:
                            if (parameters is ApiGalaxyRouteParameters galaxyParams)
                                maxTime = galaxyParams.MaxTime;
                            break;
                        default:
                            break;

                    }
                    var knownRoute = await GetResults(parameters.GetRouteType(), parameters.JobId, textUpdate, maxTime);
                    //In case it's an old/invalid job id, check if it is null
                    //If it is then just carry on as if it is a new job
                    //Otherwise return the completed one
                    if (knownRoute != null)
                    {
                        return knownRoute;
                    }
                }

                SpanshApiParameters? @params = null;
                string html = string.Empty;
                switch (parameters.GetRouteType())
                {
                    case SpanshRouteType.RoadToRiches:
                        @params = parameters as ApiRoadToRichesParameters;
                        html = RICHES_ROUTE;
                        break;
                    case SpanshRouteType.NeutronRoute:
                        @params = parameters as ApiNeutronParameters;
                        html = ROUTE;
                        break;
                    case SpanshRouteType.Colonisation:
                        @params = parameters as ApiColonisationParameters;
                        html = COLONISATION_ROUTE;
                        break;
                    case SpanshRouteType.FleetCarrier:
                        @params = parameters as ApiFleetCarrierRouteParameters;
                        html = FLEETCARRIER_ROUTE;
                        break;
                    case SpanshRouteType.GalaxyPlotter:
                        if (parameters is ApiGalaxyRouteParameters gParams)
                        {
                            @params = gParams;
                            maxTime = gParams.MaxTime;
                            html = GALAXY_ROUTE;
                        }
                        break;
                    case SpanshRouteType.WorldTypeRoute:
                        @params = parameters as ApiWorldTypeParameters;
                        html = RICHES_ROUTE;
                        break;
                    case SpanshRouteType.Exobiology:
                        @params = parameters as ApiExobiologyRouteParameters;
                        html = EXOBIOLOGY_ROUTE;
                        break;
                    case SpanshRouteType.TouristRoute:
                        @params = parameters as ApiTouristRouteApiParameters;
                        html = TOURIST_ROUTE;
                        break;
                    case SpanshRouteType.TradePlanner:
                        @params = parameters as ApiTradePlannerParameters;
                        html = TRADE_ROUTE;
                        break;
                    default:
                        return null;
                }

                if (@params == null)
                    return null;

                var headers = JObject.FromObject(@params, ignoreNull)
                           .Properties()
                           .Select(ChechForArraysAndGetString)
                           .Where(x => !string.IsNullOrEmpty(x));

                var request = string.Join("&", headers);

                var body = new StringContent(request, Encoding.ASCII, "application/x-www-form-urlencoded");
                await Task.Delay(500);
                textUpdate?.Invoke("Contacting Spansh");

                var ret = await spanshClient.PostAsync(html, body);

                if (ret.StatusCode != HttpStatusCode.Accepted)
                {
                    var json = await ret.Content.ReadAsStringAsync();
                    var jobject = JObject.Parse(json);
                    var error = jobject["error"]?.ToString();
                    switch (ret.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                            if (!string.IsNullOrEmpty(error))
                            {
                                textUpdate?.Invoke($"Spansh Returned Bad Request{Environment.NewLine}Error : {error}");
                                OnError?.Invoke(this, new HttpErrorRespone(HttpErrorResponeType.BadRequest, $"Bad Requset{Environment.NewLine}Statuscode : {ret.StatusCode}\n{ret.ReasonPhrase}"));
                                await Task.Delay(3000);
                                break;
                            }
                            textUpdate?.Invoke($"Spansh Returned Bad Request\nStatuscode : {ret.StatusCode}\n{ret.ReasonPhrase}");
                            OnError?.Invoke(this, new HttpErrorRespone(HttpErrorResponeType.BadRequest, $"Bad Requset\nStatuscode : {ret.StatusCode}\n{ret.ReasonPhrase}"));
                            //Give the user a few seconds to read the message
                            await Task.Delay(3000);
                            break;
                        default:
                            textUpdate?.Invoke($"Error Getting route\nStatuscode : {ret.StatusCode}\n{ret.ReasonPhrase}");
                            OnError?.Invoke(this, new HttpErrorRespone(HttpErrorResponeType.Unknown, $"Error Getting route\nStatuscode : {ret.StatusCode}\n{ret.ReasonPhrase}"));

                            break;
                    }
                    return null;
                }

                var responseText = await ret.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<SpanshResponseBase>(responseText);

                if (response == null)
                    return null;

                var result = await GetResults(parameters.GetRouteType(), response.Job, textUpdate, maxTime);
                return result;
            }
            catch (Exception ex)
            {
                var error = HttpErrorHelpers.GetErrorMessage(ex, "http://spansh.co.uk");
                textUpdate?.Invoke(error.Message);
                OnError?.Invoke(this, error);
                return null;
            }
        }
        //Invariant Culture so we get decimal places instead of commas
        private static string ChechForArraysAndGetString(JProperty token)
        {
            if (token.Value.Type == JTokenType.Array)
            {
                List<string> ret = [];
                foreach (var value in token.Value)
                {
                    ret.Add(string.Format(CultureInfo.InvariantCulture, "{0}={1}", token.Name, value));
                }
                return string.Join("&", ret);
            }
            if (token.Value.Type == JTokenType.Boolean)
            {
                var b = token.Value.ToObject<bool>();
                var res = b ? "1" : "0";
                return $"{token.Name}={res}";
            }
            return string.Format(CultureInfo.InvariantCulture, "{0}={1}", token.Name, token.Value);
        }

        private async Task<SpanshResponseBase?> GetResults(SpanshRouteType type, string job, Action<string>? textUpdate, int maxTimeSeconds = 60)
        {
            return type switch
            {
                SpanshRouteType.RoadToRiches => await GetResultsActual<ApiRoadToRichesRoute>(job, textUpdate, maxTimeSeconds),
                SpanshRouteType.NeutronRoute => await GetResultsActual<ApiNeutronRoute>(job, textUpdate, maxTimeSeconds),
                SpanshRouteType.Colonisation => await GetResultsActual<ApiColonisationRoute>(job, textUpdate, maxTimeSeconds),
                SpanshRouteType.FleetCarrier => await GetResultsActual<ApiFleetCarrierRoute>(job, textUpdate, maxTimeSeconds),
                SpanshRouteType.GalaxyPlotter => await GetResultsActual<ApiGalaxyPlotterRoute>(job, textUpdate, maxTimeSeconds),
                SpanshRouteType.WorldTypeRoute => await GetResultsActual<ApiWorldTypeRoute>(job, textUpdate, maxTimeSeconds),
                SpanshRouteType.Exobiology => await GetResultsActual<ApiExoviologyRoute>(job, textUpdate, maxTimeSeconds),
                SpanshRouteType.TouristRoute => await GetResultsActual<ApiTouristRoute>(job, textUpdate, maxTimeSeconds),
                SpanshRouteType.TradePlanner => await GetResultsActual<ApiTradePlannerRoute>(job, textUpdate, maxTimeSeconds),
                _ => null,
            };
        }
        private async Task<T?> GetResultsActual<T>(string job, Action<string>? textUpdate, int maxTimeSeconds = 60) where T : SpanshResponseBase
        {
            await Task.Delay(1000);
            textUpdate?.Invoke("Waiting for Results");
            await Task.Delay(9000);
            T? response = null;
            string json = string.Empty;
            try
            {
                JsonSerializerSettings ignoreNull = new()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
                };

                var pollIntervalMs = 5000;
                var elasped = TimeSpan.Zero;
                var elaspedLimit = TimeSpan.FromSeconds(maxTimeSeconds);
                var startTime = DateTime.UtcNow;
                while (elasped < elaspedLimit)
                {
                    json = await spanshClient.GetStringAsync($"{RESULTS}{job}");
                    response = JsonConvert.DeserializeObject<T>(json, ignoreNull);

                    if (response != null && response.State == "completed" && response.Status == "ok")
                    {
                        textUpdate?.Invoke($"State : {response.State}{Environment.NewLine}Status : {response.Status}");
                        await Task.Delay(1000);
                        return response;
                    }
                    if (response != null)
                        textUpdate?.Invoke($"State  : {response.State}{Environment.NewLine}Status : {response.Status}");
                    await Task.Delay(pollIntervalMs);
                    elasped = DateTime.UtcNow - startTime;
#if DEBUG
                    Debug.WriteLine($"Elased Time on Spansh Query {elasped.TotalSeconds}");
#endif
                }

                textUpdate?.Invoke("Timed Out Getting Respone");
                await Task.Delay(5000);
                return response;
            }
            catch (Exception ex)
            {
                var error = HttpErrorHelpers.GetErrorMessage(ex, "http://spansh.co.uk");
                OnError?.Invoke(this, error);
                return response;
            }
        }


    }
}
