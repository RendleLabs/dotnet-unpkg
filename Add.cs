using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace dotnet_unpkg
{
    public static class Add
    {
        private static readonly Regex DistInPath = new Regex(@"\/dist\/.*");
        private static readonly HttpClient Client = new HttpClient
        {
            BaseAddress = new Uri("https://unpkg.com")
        };
        
        private static readonly string BaseDirectory = Path.Combine("wwwroot", "lib");

        public static async Task Run(IEnumerable<string> args)
        {
            var argList = args.ToList();
            if (argList[0] == "--help" || argList[0] == "-h")
            {
                Help.Add();
                return;
            }
            
            if (!Directory.Exists(BaseDirectory))
            {
                Directory.CreateDirectory(BaseDirectory);
            }

            var results = await Task.WhenAll(argList.Select(AddPackage));
            await UnpkgJson.Save(results);
        }

        private static async Task<UnpkgJsonEntry> AddPackage(string package)
        {
            var distFile = await Dist.Get(package);
            if (distFile == null)
            {
                return null;
            }

            var distDirectory = distFile.Files.FirstOrDefault(f =>
                f.Type.Equals("directory", StringComparison.OrdinalIgnoreCase) && f.Path.Equals("/dist", StringComparison.OrdinalIgnoreCase));

            if (distDirectory != null)
            {
                distDirectory.BaseUrl = distFile.BaseUrl;
                distFile = distDirectory;
            }

            await DownloadPackage(package, distFile.BaseUrl, distFile.Files);
            return UnpkgJsonEntry.Create(package, distFile);
        }

        private static Task DownloadPackage(string package, string basePath, IEnumerable<DistFile> files)
        {
            basePath = DistInPath.Replace(basePath, string.Empty);
            var tasks = new List<Task>();
            foreach (var file in files)
            {
                if (file.Type == "file")
                {
                    tasks.Add(DownloadFile(package, basePath, file));
                }
                else if (file.Files?.Count > 0)
                {
                    tasks.Add(DownloadPackage(package, basePath, file.Files));
                }
            }

            return Task.WhenAll(tasks);
        }

        private static async Task DownloadFile(string package, string basePath, DistFile file)
        {
            var (cdn, localPath) = await Download.DistFile(package, $"{basePath.TrimSlashes()}/{file.Path.TrimSlashes()}");
            file.Url = cdn;
            file.LocalPath = localPath.Replace(Path.DirectorySeparatorChar, '/');
        }
    }
}
