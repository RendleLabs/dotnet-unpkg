using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace dotnet_unpkg
{
    public static class Download
    {
        private static readonly char[] SplitChar = {'/', '\\'};
        private static readonly HttpClient Client = new HttpClient
        {
            BaseAddress = new Uri("https://unpkg.com")
        };
        private static readonly string BaseDirectory = Path.Combine(Settings.Wwwroot, "lib");
        
        public static async Task<(string, string)> DistFile(string package, string path)
        {
            var packageSegments = package.Split(SplitChar, StringSplitOptions.RemoveEmptyEntries);
            var target = TargetFile(package, path);

            if (packageSegments.Length > 1)
            {
                var targetSegments = target.Split(SplitChar, StringSplitOptions.RemoveEmptyEntries);
                if (targetSegments.Length > 1)
                {
                    if (packageSegments.Last() == targetSegments.First())
                    {
                        target = string.Join(Path.DirectorySeparatorChar, targetSegments.Skip(1));
                    }
                }
            }

            for (int i = 0; i < packageSegments.Length; i++)
            {
                if (packageSegments[i].Contains('@') && !packageSegments[i].StartsWith('@'))
                {
                    packageSegments[i] = packageSegments[i].Split('@')[0];
                }
            }

            package = string.Join('/', packageSegments);

            using (var response = await Client.GetAsync(path))
            {
                if (response.IsSuccessStatusCode)
                {
                    var file = Path.GetFileName(target);
                    var directory = Path.Combine(BaseDirectory, package, Path.GetDirectoryName(target));

                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    var localPath = Path.Combine(directory, file);
                    using (var fileStream = File.Create(localPath))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }

                    Console.WriteLine($"{response.RequestMessage.RequestUri}... OK");
                    return (response.RequestMessage.RequestUri.ToString(), localPath);
                }
                else
                {
                    Console.WriteLine($"{response.RequestMessage.RequestUri}... failed ({(int)response.StatusCode})");
                    return default;
                }
            }
        }

        public static async Task RestoreDistFile(string url, string path)
        {
            using (var response = await Client.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    path = path.Replace('/', Path.DirectorySeparatorChar);
                    var directory = Path.GetDirectoryName(path);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    using (var fileStream = File.Create(path))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }

                    Console.WriteLine($"{response.RequestMessage.RequestUri}... OK");
                }
                else
                {
                    Console.WriteLine($"{response.RequestMessage.RequestUri}... failed ({(int)response.StatusCode})");
                }
            }
        }
        
        private static string TargetFile(string package, string path)
        {
            var packageEnd = package.Split(SplitChar, StringSplitOptions.RemoveEmptyEntries).Last();
            var pathParts = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries).ToArray();
            
            if (pathParts.Contains("dist"))
            {
                pathParts = pathParts
                        .SkipWhile(s => !s.Equals("dist", StringComparison.OrdinalIgnoreCase))
                        .Skip(1).ToArray();

                switch (pathParts.Length)
                {
                    case 0:
                        return path;
                    case 1:
                        return pathParts[0];
                    default:
                        if (pathParts[0].Equals(packageEnd, StringComparison.OrdinalIgnoreCase))
                        {
                            pathParts = pathParts.Skip(1).ToArray();
                        }

                        break;
                }
            }
            else
            {
                pathParts = pathParts.Skip(1).ToArray();
            }

            return string.Join(Path.DirectorySeparatorChar, pathParts);
        }
    }
}