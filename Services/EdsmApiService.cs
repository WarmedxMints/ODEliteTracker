using EliteJournalReader;
using System.Net.Http;
using System.Net.Http.Json;

namespace ODEliteTracker.Services
{
    public class EdsmBody
    {
        public long Id { get; set; }
        public long Id64 { get; set; }
        public string? Name { get; set; }
    }

    public class EdsmBodyResponse
    {
        public string? Name { get; set; }
        public string? Url { get; set; }
        public int? BodyCount { get; set; }
        public IReadOnlyList<EdsmBody>? Bodies { get; set; }
    }

    public class EdsmStar
    {
        public string? Type { get; set; }
        public string? Name { get; set; }
        public bool IsScoopable { get; set; }
    }

    public class EdsmPrimaryStarResponse
    {
        public string? Name { get; set; }
        public EdsmStar? PrimaryStar { get; set; }
    }

    public class EdsmValueResponse
    {
        public int Id { get; set; }
        public long Id64 { get; set; }
        public string? Name { get; set; }
        public string? Url { get; set; }
        public long EstimatedValue { get; set; }
        public long EstimatedValueMapped { get; set; }
        public IReadOnlyCollection<EdsmValuableBody>? ValuableBodies { get; set; }
    }

    public class EdsmValuableBody
    {
        public int BodyId { get; set; }
        public string? BodyName { get; set; }
        public double Distance { get; set; }
        public int ValueMax { get; set; }
    }

    public sealed class EdsmApiService(HttpClient httpClient)
    {
        private readonly HttpClient httpClient = httpClient;
        public event EventHandler<Exception>? OnError;
        public async Task<string?> GetSystemUrlAsync(ulong address)
        {
            var content = await httpClient.GetFromJsonAsync<EdsmBodyResponse>($"api-system-v1/bodies?systemId64={address}").ConfigureAwait(true);

            if (content?.Url is null)
                return null;
            return content.Url.Replace("&", "^&");
        }

        public async Task<Tuple<int, int>> GetBodyCountAsync(long address)
        {
            try
            {
                var bodyCount = -1;
                var bodiesCount = -1;
                var content = await httpClient.GetFromJsonAsync<EdsmBodyResponse>($"api-system-v1/bodies?systemId64={address}").ConfigureAwait(true);

                if (content?.BodyCount is not null)
                {
                    bodyCount = (int)content.BodyCount;
                }

                if (content?.Bodies is not null)
                {
                    bodiesCount = content.Bodies.Count;
                }
                return Tuple.Create(bodyCount, bodiesCount);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
                return Tuple.Create(-1, -1);
            }
        }

        public async Task<StarType> GetPrimaryStarClassAsync(string systemName)
        {
            try
            {
                var content = await httpClient.GetFromJsonAsync<EdsmPrimaryStarResponse>($"api-v1/system?systemName={System.Web.HttpUtility.UrlEncode(systemName)}&showPrimaryStar=1").ConfigureAwait(true);

                if (string.IsNullOrEmpty(content?.PrimaryStar?.Type))
                {
                    return StarType.Unknown;
                }

                var ret = ODMVVM.Helpers.EnumHelpers.GetEnumValueFromDescription<StarType>(content.PrimaryStar.Type);

                if (ret != StarType.Unknown)
                    return ret;

                string[] sClass = content.PrimaryStar.Type.Split(' ');

                if (Enum.TryParse(sClass[0], out StarType result))
                {
                    return result;
                }
                return StarType.Unknown;
            }
            catch (System.Text.Json.JsonException ex)
            {
                OnError?.Invoke(this, ex);
                return StarType.Unknown;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
                return StarType.Unknown;
            }
        }

        public async Task<StarType> GetPrimaryStarClass(string systemName)
        {
            var content = await httpClient.GetFromJsonAsync<EdsmPrimaryStarResponse>($"api-v1/system?systemName={System.Web.HttpUtility.UrlEncode(systemName)}&showPrimaryStar=1").ConfigureAwait(true);

            if (string.IsNullOrEmpty(content?.PrimaryStar?.Type))
            {
                return StarType.Unknown;
            }

            string[] sClass = content.PrimaryStar.Type.Split(' ');

            if (Enum.TryParse(sClass[0], out StarType result))
            {
                return result;
            }
            return StarType.Unknown;
        }

        public async Task<EdsmValueResponse?> GetSystemValueAsync(string systemName)
        {
            var content = await httpClient.GetFromJsonAsync<EdsmValueResponse>($"api-system-v1/estimated-value?systemName={System.Web.HttpUtility.UrlEncode(systemName)}").ConfigureAwait(true);

            if (string.IsNullOrEmpty(content?.Name))
                return null;

            return content;
        }
    }
}
