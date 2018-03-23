using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace dotnet_unpkg
{
    public class Dist
    {
        private static readonly Regex EndPart = new Regex(@"\/dist\/\?meta$");
        private static readonly HttpClient Client = CreateClient();
        
        public static async Task<DistFile> Get(string package)
        {
            var url = $"https://unpkg.com/{package}/dist/?meta";
            var response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            while (response.StatusCode == HttpStatusCode.Redirect)
            {
                Console.WriteLine(response.Headers.Location);
                url = response.Headers.Location.ToString();
                response = await Client.GetAsync(url);
            }
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var distFile = JsonConvert.DeserializeObject<DistFile>(json);
                distFile.BaseUrl = EndPart.Replace(url, string.Empty);
                return distFile;
            }

            Console.Error.WriteLine($"{url} returned status {(int)response.StatusCode}.");
            return null;
        }

        private static HttpClient CreateClient()
        {
            var httpMessageHandler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
            };
            var client = new HttpClient(httpMessageHandler)
            {
                BaseAddress = new Uri("https://unpkg.com")
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }
}