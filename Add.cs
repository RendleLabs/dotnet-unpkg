using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace dotnet_unpkg
{
    static class UnpkgJson
    {
        
    }

    static class Add
    {
        private static readonly HttpClient Client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
        private static readonly string BaseDirectory = Path.Combine("wwwroot", "lib");

        public static async Task<ICollection<string>> Run(IEnumerable<string> args)
        {
            if (!Directory.Exists(BaseDirectory))
            {
                Directory.CreateDirectory(BaseDirectory);
            }

            var results = await Task.WhenAll(args.Select(AddPackage));
            return results;
        }

        private static async Task<string> AddPackage(string package)
        {
            var url = $"https://unpkg.com/{package}";
            var response = await Client.GetAsync(url);

            while (response.StatusCode == HttpStatusCode.Redirect)
            {
                url = $"https://unpkg.com{response.Headers.Location}";
                response = await Client.GetAsync(url);
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine($"Downloading {url}...");
                string packageDirectory = EnsureDirectory(package, url);

                Uri uri = new Uri(url);
                var filename = Path.GetFileName(uri.LocalPath);
                var filepath = Path.Combine(packageDirectory, filename);
                using (var file = File.Create(filepath))
                using (var body = await response.Content.ReadAsStreamAsync())
                {
                    await body.CopyToAsync(file);
                }

                await TryMin(url, response, packageDirectory);

                Console.WriteLine($"{package}: {filepath}");
                return filename;
            }

            Console.WriteLine($"{package}: not found.");
            return $"{package} not found.";
        }

        private static async Task TryMin(string url, HttpResponseMessage response, string packageDirectory)
        {
            string minUrl;
            if (url.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
            {
                minUrl = Regex.Replace(url, @"\.js$", ".min.js");
            }
            else if (url.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
            {
                minUrl = Regex.Replace(url, @"\.css$", ".min.css");
            }
            else
            {
                return;
            }

            response = await Client.GetAsync(minUrl);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine($"Downloading {minUrl}...");
                var minFilename = Path.GetFileName(new Uri(minUrl).LocalPath);
                var minFilepath = Path.Combine(packageDirectory, minFilename);
                using (var file = File.Create(minFilepath))
                using (var body = await response.Content.ReadAsStreamAsync())
                {
                    await body.CopyToAsync(file);
                }
            }
        }

        private static string EnsureDirectory(string package, string url)
        {
            string packageDirectory = Path.Combine(BaseDirectory, package);
            if (package.Contains('/'))
            {
                var packageFileName = Path.GetFileName(package);
                var foundFileName = Path.GetFileName(new Uri(url).LocalPath);
                if (string.Equals(packageFileName, foundFileName, StringComparison.OrdinalIgnoreCase))
                {
                    packageDirectory = Path.Combine(BaseDirectory, Path.GetDirectoryName(package));
                }
            }
            if (!Directory.Exists(packageDirectory))
            {
                Directory.CreateDirectory(packageDirectory);
            }

            return packageDirectory;
        }
    }
}
