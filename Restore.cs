using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
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
                .Select(f => DownloadFile(package, version, (JObject)f)));
        }

        private static Task DownloadFile(string package, string version, JObject file)
        {
            var local = file["local"].Value<string>();
            if (File.Exists(local))
            {
                var integrity = file["integrity"].Value<string>();
                var integrityBits = integrity.Split('-', 2);
                if (integrityBits.Length == 2)
                {
                    var hashAlgorithm = GetAlgorithm(integrityBits[0]);
                    if (hashAlgorithm != null)
                    {
                        using (var stream = File.OpenRead(local))
                        {
                            var hash = hashAlgorithm.ComputeHash(stream);
                            if (integrityBits[1].Equals(Convert.ToBase64String(hash)))
                            {
                                Console.WriteLine($"{local} is up-to-date.");
                                return Task.CompletedTask;
                            }
                        }
                    }
                }
            }

            return Download.DistFile(package, $"{version}/{file["file"].Value<string>()}");
        }

        private static HashAlgorithm GetAlgorithm(string name)
        {
            switch (name.ToLowerInvariant())
            {
                case "sha256":
                    return SHA256.Create();
                case "sha384":
                    return SHA384.Create();
                case "sha512":
                    return SHA512.Create();
                default:
                    return null;
            }
        }
    }
}