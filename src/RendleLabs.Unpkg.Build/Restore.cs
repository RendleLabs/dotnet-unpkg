using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RendleLabs.Unpkg.Build
{
    public static class Restore
    {
        public static Task<RestoreResults> Run()
        {
            return Run(Environment.CurrentDirectory, "unpkg.json");
        }

        public static async Task<RestoreResults> Run(string directory, string unpkgFile)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            if (unpkgFile == null) throw new ArgumentNullException(nameof(unpkgFile));

            var fullFilePath = Path.Combine(directory, unpkgFile);
            
            if (!File.Exists(fullFilePath))
            {
                return new RestoreResults {Error = $"No {unpkgFile} file found in current directory."};
            }

            string json;
            using (var reader = File.OpenText(fullFilePath))
            {
                json = await reader.ReadToEndAsync();
            }

            JObject file;
            try
            {
                file = JObject.Parse(json);
            }
            catch
            {
                return new RestoreResults {Error = $"Error parsing {unpkgFile}."};
            }

            var allFiles = await Task.WhenAll(file.Properties().Select(p => DownloadFiles((JObject) p.Value)));

            return new RestoreResults {Results = allFiles.SelectMany(f => f).ToArray()};
            
        }

        private static Task<RestoreResult[]> DownloadFiles(JObject entry)
        {
            var files = (JArray) entry["files"];

            return Task.WhenAll(files
                .Select(f => DownloadFile((JObject)f)));
        }

        private static async Task<RestoreResult> DownloadFile(JObject file)
        {
            var local = file["local"]?.Value<string>()?.Replace('/', Path.DirectorySeparatorChar);
            var cdn = file["cdn"]?.Value<string>();
            
            if (string.IsNullOrWhiteSpace(local) || string.IsNullOrWhiteSpace(cdn))
            {
                return new RestoreResult {Error = $"Could not restore: {cdn}"};
            }

            if (!File.Exists(local)
                || !TryGetHashAlgorithm(file["integrity"].Value<string>(), out var hashAlgorithm, out var storedHash)
                || !storedHash.Equals(GetCurrentFileHash(local, hashAlgorithm)))
            {
                await Download.RestoreDistFile(cdn, local);
                return new RestoreResult {CdnUrl = cdn, LocalFile = local};
            }
            
            return new RestoreResult {Message = $"{local} is up-to-date."};

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
                var integrityBits = integrity.Split(new[]{'-'}, 2);
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

    public class RestoreResults
    {
        public RestoreResult[] Results { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
    }

    public class RestoreResult
    {
        public string LocalFile { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
        public string CdnUrl { get; set; }
    }
}