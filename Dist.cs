using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace dotnet_unpkg
{
    public static class Dist
    {
        private static readonly Regex EndPart = new Regex(@"\/dist\/\?meta$");
        private static readonly HttpClient Client = CreateClient();
        
        public static async Task<DistFile> Get(string package)
        {
            var response = await Find(package);
            var url = response.RequestMessage.RequestUri.AbsolutePath;
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Found {UnpkgJson.CleanVersion(url)}");
                var json = await response.Content.ReadAsStringAsync();
                var distFile = JsonConvert.DeserializeObject<DistFile>(json);
                distFile.BaseUrl = EndPart.Replace(url, string.Empty);
                return distFile;
            }

            Console.Error.WriteLine($"{url} returned status {(int)response.StatusCode}.");
            return null;
        }

        private static async Task<HttpResponseMessage> Find(string package)
        {
            string url;
            var parts = package.Split('/');
            string sub = null;

            if (package.StartsWith('@') && parts.Length > 1)
            {
                package = $"{parts[0]}/{parts[1]}";
                if (parts.Length > 2)
                {
                    sub = string.Join('/', parts.Skip(2));
                }
            }
            else if (parts.Length > 1)
            {
                package = parts[0];
                sub = string.Join('/', parts.Skip(1));
            }

            if (sub != null)
            {
                url = $"{package}/dist/{sub}/?meta";
            }
            else
            {
                url = $"{package}/dist/?meta";
            }
            
            var response = await FollowRedirects(url);
            
            if (response.IsSuccessStatusCode)
            {
                return response;
            }
            
            response.Dispose();

            return await FollowRedirects($"{package}/?meta");
        }

        private static async Task<HttpResponseMessage> FollowRedirects(string url)
        {
            var response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            
            while (response.StatusCode == HttpStatusCode.Redirect)
            {
                url = response.Headers.Location.ToString();
                response.Dispose();
                response = await Client.GetAsync(url);
            }

            return response;
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