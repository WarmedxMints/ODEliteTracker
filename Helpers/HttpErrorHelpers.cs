using System.Net.Http;

namespace ODEliteTracker.Helpers
{
    public enum HttpErrorResponeType
    {
        Ignorable,
        Unknown,
        Firewall,
        _404,
        _500,
        BadRequest,
    }

    internal record HttpErrorRespone(HttpErrorResponeType ErrorType, string Message);
    internal static class HttpErrorHelpers
    {
        private static string FirewallError => $"It appeaers there is a firewall blocking OD Elite Tracker{DoubleNewLine}Adding {AppDomain.CurrentDomain.BaseDirectory} to your firwall may help{DoubleNewLine}Would you like to copy that to the clipboard?";
        private static string Unknown => "An unknown error has occured";
        private static string DoubleNewLine => $"{Environment.NewLine}{Environment.NewLine}";
        internal static HttpErrorRespone GetErrorMessage(Exception? ex, string baseUrl)
        {
            if (ex != null && !ex.Message.Contains("404"))
                return new(HttpErrorResponeType._404, $"404 Error when attempting to contact {baseUrl}");

            if (ex == null)
                return new(HttpErrorResponeType.Unknown, $"{Unknown} when attempting to contact {baseUrl}");            

            if (ex.Message.Contains("An attempt was made to access a socket in a way forbidden by its access permissions")
                || ex.Message.Contains("The SSL connection could not be established")
                || ex.Message.Contains("A socket operation was attempted to an unreachable network"))
            {
                return new(HttpErrorResponeType.Firewall, FirewallError);
            }

            if (ex is HttpRequestException httpex && httpex.StatusCode == System.Net.HttpStatusCode.InternalServerError
                    || ex.Message.Contains("Response status code does not indicate success: 500")
                    || ex.Message.Contains("(Internal Server Error)"))
            {
                return new(HttpErrorResponeType._500, $"500 Error (Internal Server Error) when attempting to contact {baseUrl}");
            }

            //Something we haven't thought of
            return new(HttpErrorResponeType.Ignorable, string.Empty);
        }
    }
}
