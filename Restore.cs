using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace dotnet_unpkg
{
    public static class Restore
    {
        private static readonly HttpClient Client = new HttpClient
        {
            BaseAddress = new Uri("https://unpkg.com")
        };

        public static async Task Run(IEnumerable<string> args)
        {
            var argList = args.ToList();
            if (argList.Count > 0 && (argList[0] == "--help" || argList[0] == "-h"))
            {
                Help.Restore();
                return;
            }

            if (!File.Exists("unpkg.json"))
            {
                Console.Error.WriteLine("No unpkg.json file found in current directory.");
                return;
            }

            string json;
            using (var reader = File.OpenText("unpkg.json"))
            {
                json = await reader.ReadToEndAsync();
            }

            var file = JObject.Parse(json);

            await Task.WhenAll(file.Properties().Select(p => DownloadFiles(p.Name, (JObject) p.Value)));
        }

        private static Task DownloadFiles(string package, JObject entry)
        {
            if (package.Contains('/'))
            {
                package = package.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            }

            var version = entry["version"].Value<string>();

            var files = (JArray) entry["files"];

            return Task.WhenAll(files
                .Select(f => f["file"].Value<string>())
                .Select(f => Download.DistFile(package, $"{version}/{f}")));
        }
    }
}