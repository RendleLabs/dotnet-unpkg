using System.Collections.Generic;
using RendleLabs.Unpkg.Annotations;

namespace RendleLabs.Unpkg
{
    [PublicAPI]
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
            AddFiles(entry.Files, file.Files);
            return entry;
        }

        private static void AddFiles(List<UnpkgJsonFile> files, IEnumerable<DistFile> distFiles)
        {
            foreach (var distFile in distFiles)
            {
                if (distFile.Type == "file")
                {
                    files.Add(new UnpkgJsonFile
                    {
                        Path = distFile.Path,
                        LocalPath = distFile.LocalPath,
                        CdnUrl = distFile.Url,
                        Integrity = distFile.Integrity
                    });
                }
                else if (distFile.Files?.Count > 0)
                {
                    AddFiles(files, distFile.Files);
                }
            }
        }
    }
}