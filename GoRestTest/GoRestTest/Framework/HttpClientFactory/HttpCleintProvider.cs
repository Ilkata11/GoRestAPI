using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace GoRestTest.Framework
{
    public class HttpClientProvider
    {
        public HttpClient Client { get; }

        public HttpClientProvider(string baseUrl, string token)
        {
            Client = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };

            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
