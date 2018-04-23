using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace dotnet_unpkg
{
    public static class Restore
    {
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

            await Task.WhenAll(file.Properties().Select(p => DownloadFiles((JObject) p.Value)));
        }

        private static Task DownloadFiles(JObject entry)
        {
            var files = (JArray) entry["files"];

            return Task.WhenAll(files
                .Select(f => DownloadFile((JObject)f)));
        }

        private static Task DownloadFile(JObject file)
        {
            var local = file["local"]?.Value<string>()?.Replace('/', Path.DirectorySeparatorChar);
            var cdn = file["cdn"]?.Value<string>();
            
            if (string.IsNullOrWhiteSpace(local) || string.IsNullOrWhiteSpace(cdn))
            {
                Console.Error.WriteLine($"Could not restore: {file}");
                return Task.CompletedTask;
            }

            if (!File.Exists(local)
                || !TryGetHashAlgorithm(file["integrity"].Value<string>(), out var hashAlgorithm, out var storedHash)
                || !storedHash.Equals(GetCurrentFileHash(local, hashAlgorithm)))
            {
                return Download.RestoreDistFile(cdn, local);
            }
            
            Console.WriteLine($"{local} is up-to-date.");
            return Task.CompletedTask;

        }
        
        private static string GetCurrentFileHash(string local, HashAlgorithm hashAlgorithm)
        {
            using (var stream = File.OpenRead(local))
            {
                return Convert.ToBase64String(hashAlgorithm.ComputeHash(stream));
            }
        }

        private static bool TryGetHashAlgorithm(string integrity, out HashAlgorithm hashAlgorithm, out string storedHash)
        {
            if (!string.IsNullOrWhiteSpace(integrity))
            {
                var integrityBits = integrity.Split('-', 2);
                if (integrityBits.Length == 2)
                {
                    hashAlgorithm = GetAlgorithm(integrityBits[0]);
                    storedHash = integrityBits[1];
                    return hashAlgorithm != null;
                }
            }

            hashAlgorithm = default;
            storedHash = default;
            return false;
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