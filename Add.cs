using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace dotnet_unpkg
{
    static class UnpkgJson
    {
        
    }

    static class Add
    {
        private static readonly HttpClient Client = new HttpClient
        {
            BaseAddress = new Uri("https://unpkg.com")
        };
        
        private static readonly string BaseDirectory = Path.Combine("wwwroot", "lib");

        public static async Task<List<UnpkgJsonEntry>> Run(IEnumerable<string> args)
        {
            if (!Directory.Exists(BaseDirectory))
            {
                Directory.CreateDirectory(BaseDirectory);
            }

            var results = await Task.WhenAll(args.Select(AddPackage));
            return results.ToList();
        }

        private static async Task<UnpkgJsonEntry> AddPackage(string package)
        {
            var distFile = await Dist.Get(package);
            if (distFile == null)
            {
                return null;
            }

            await Download(package, distFile.BaseUrl, distFile.Files);
            return UnpkgJsonEntry.Create(package, distFile);
        }

        private static Task Download(string package, string basePath, IEnumerable<DistFile> files)
        {
            var tasks = new List<Task>();
            foreach (var file in files)
            {
                if (file.Type == "file")
                {
                    tasks.Add(Download(package, basePath, file.Path));
                }
                else if (file.Files?.Count > 0)
                {
                    tasks.Add(Download(package, basePath, file.Files));
                }
            }

            return Task.WhenAll(tasks);
        }

        private static async Task Download(string package, string basePath, string path)
        {
            using (var response = await Client.GetAsync($"{basePath}{path}"))
            {
                if (response.IsSuccessStatusCode)
                {
                    // Remove /dist/ from start of path
                    path = path.Substring(6);
                    
                    if (Path.DirectorySeparatorChar != '/')
                    {
                        path = path.Replace('/', Path.DirectorySeparatorChar);
                    }
                
                    var file = Path.GetFileName(path);
                    var directory = Path.Combine(BaseDirectory, package, Path.GetDirectoryName(path));

                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    using (var fileStream = File.Create(Path.Combine(directory, file)))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }
                }
            }
        }
    }
}
