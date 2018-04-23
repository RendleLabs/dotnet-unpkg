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
                var jEntry = CreateSaveObject(entry);
                file[CleanPackageName(entry.PackageName)] = jEntry;
            }

            using (var writer = File.CreateText("unpkg.json"))
            {
                await writer.WriteAsync(file.ToString(Formatting.Indented));
            }
        }

        private static JObject CreateSaveObject(UnpkgJsonEntry entry)
        {
            var jEntry = new JObject
            {
                ["version"] = ExtractVersion(entry.Version),
                ["files"] = JArray.FromObject(entry.Files.Select(f =>
                    new {file = f.Path, cdn = f.CdnUrl, local = f.LocalPath, integrity = f.Integrity}))
            };
            return jEntry;
        }

        public static async Task<UnpkgJsonEntry[]> Load()
        {
            var file = await LoadJson();
            return file == null ? Array.Empty<UnpkgJsonEntry>() : Parse(file.Properties()).ToArray();
        }

        private static IEnumerable<UnpkgJsonEntry> Parse(IEnumerable<JProperty> properties)
        {
            return properties.Select(property => new UnpkgJsonEntry
            {
                Version = property.Value["version"].Value<string>(),
                PackageName = property.Name,
                Files = property.Value["files"].Values<JObject>()
                    .Select(ParseUnpkgJsonFile).ToList()
            });
        }

        private static UnpkgJsonFile ParseUnpkgJsonFile(JObject f)
        {
            return new UnpkgJsonFile
            {
                Path = f["file"].Value<string>(),
                CdnUrl = f["cdn"].Value<string>(),
                LocalPath = f["local"].Value<string>(),
                Integrity = f["integrity"].Value<string>()
            };
        }

        private static async Task<JObject> LoadJson()
        {
            if (!File.Exists("unpkg.json")) return default;
            using (var reader = File.OpenText("unpkg.json"))
            {
                var content = await reader.ReadToEndAsync();
                return JObject.Parse(content);
            }
        }

        private static string CleanPackageName(string full)
        {
            var segments = full.Split('/');
            
            for (int i = 0, l = segments.Length; i < l; i++)
            {
                if (segments[i].IndexOf('@') > 0)
                {
                    segments[i] = segments[i].Split('@')[0];
                }
            }

            return string.Join('/', segments);
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