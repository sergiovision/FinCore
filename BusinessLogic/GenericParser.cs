using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public abstract class GenericParser
    {
        public const string UserAgent =
            "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; Zune 4.0; InfoPath.3; MS-RTC LM 8; .NET4.0C; .NET4.0E)";

        protected bool? bUseInterval;
        protected bool doHistoryParsing;

        protected DateTime IntervalEndDate;
        protected DateTime IntervalStartDate;

        public GenericParser(bool parseHistory)
        {
            doHistoryParsing = parseHistory;
        }

        public static async Task<string> GetDataRequest(string url, bool useAgent)
        {
            var httpClient = new HttpClient();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            if (useAgent) httpRequestMessage.Headers.Add("User-Agent", UserAgent);
            HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
    }
}