using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace dotnet_unpkg
{
    public static class UnpkgJson
    {
        public static async Task Save(IEnumerable<UnpkgJsonEntry> entries)
        {
            JObject file;
            if (File.Exists("unpkg.json"))
            {
                using (var reader = File.OpenText("unpkg.json"))
                {
                    var content = await reader.ReadToEndAsync();
                    file = JObject.Parse(content);
                }
            }
            else
            {
                file = new JObject();
            }

            foreach (var entry in entries)
            {
                var jEntry = new JObject
                {
                    ["version"] = ExtractVersion(entry.Version),
                    ["files"] = JArray.FromObject(entry.Files.Select(f =>
                        new {file = f.Path, cdn = f.CdnUrl, local = f.LocalPath, integrity = f.Integrity}))
                };
                file[CleanPackageName(entry.PackageName)] = jEntry;
            }

            using (var writer = File.CreateText("unpkg.json"))
            {
                await writer.WriteAsync(file.ToString(Formatting.Indented));
            }
        }

        private static string CleanPackageName(string full)
        {
            var last = full.LastIndexOf('@');
            return last > 0 ? full.Substring(0, last) : full;
        }

        public static string ExtractVersion(string full)
        {
            var parts = full.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var fileAtVersion = parts
                .FirstOrDefault(s => s[0] != '@' && s.Contains('@'));
            if (fileAtVersion == null) return parts[0];
            parts = fileAtVersion.Split('@', 2, StringSplitOptions.RemoveEmptyEntries);
            return parts[1];
        }
    }
}