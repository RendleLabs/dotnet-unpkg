using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RendleLabs.Unpkg
{
    public static class Upgrade
    {
        private static readonly string BaseDirectory = Path.Combine("wwwroot", "lib");

        public static async Task Run(IEnumerable<string> args)
        {
            var argList = args.ToList();
            HashSet<string> packages = null;
            if (argList.Count > 0)
            {
                if (argList[0] == "--help" || argList[0] == "-h")
                {
                    Help.Upgrade();
                    return;
                }
                else
                {
                    packages = new HashSet<string>(argList, StringComparer.OrdinalIgnoreCase);
                }
            }

            if (!Directory.Exists(BaseDirectory))
            {
                Directory.CreateDirectory(BaseDirectory);
            }

            var entries = await UnpkgJson.Load();

            if (packages != null)
            {
                entries = entries.Where(e => packages.Contains(e.PackageName)).ToArray();
            }

            entries = (await Task.WhenAll(entries.Select(GetDistFile)))
                .Where((e, d) => VersionComparison.IsGreater(d.Version, e.Version))
                .Select((e, _) => e)
                .ToArray();

            foreach (var entry in entries)
            {
                foreach (var file in entry.Files)
                {
                    if (File.Exists(file.LocalPath))
                    {
                        File.Delete(file.LocalPath);
                    }
                }
            }

            await Add.Run(entries.Select(e => e.PackageName));
        }

        private static async Task<(UnpkgJsonEntry, DistFile)> GetDistFile(UnpkgJsonEntry entry)
        {
            return (entry, await Dist.Get(entry.PackageName));
        }
    }
}