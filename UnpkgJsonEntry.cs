using System.Collections.Generic;

namespace dotnet_unpkg
{
    public class UnpkgJsonEntry
    {
        public string PackageName { get; set; }
        public string Version { get; set; }
        public List<UnpkgJsonFile> Files { get; set; }

        public static UnpkgJsonEntry Create(string packageName, DistFile file)
        {
            var entry = new UnpkgJsonEntry
            {
                PackageName = packageName,
                Version = file.BaseUrl,
                Files = new List<UnpkgJsonFile>()
            };
            AddFiles(entry.Files, file.BaseUrl, file.Files);
            return entry;
        }

        private static void AddFiles(List<UnpkgJsonFile> files, string version, IEnumerable<DistFile> distFiles)
        {
            foreach (var distFile in distFiles)
            {
                if (distFile.Type == "file")
                {
                    files.Add(new UnpkgJsonFile { Path = distFile.Path, CdnUrl = $"https://unpkg.com/{version}{distFile.Path}", Integrity = distFile.Integrity});
                }
                else if (distFile.Files?.Count > 0)
                {
                    AddFiles(files, version, distFile.Files);
                }
            }
        }
    }
}