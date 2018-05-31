using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace RendleLabs.Unpkg
{
    public static class Settings
    {
        public static void Initialize(string[] args)
        {
            var localConfig = Path.Combine(Environment.CurrentDirectory, "unpkg.config");
            var userConfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".unpkg", "unpkg.config");

            Wwwroot = TryCommandLineArgs(args, "wwwroot")
                      ?? TryJsonFile(localConfig, "wwwroot")
                      ?? TryEnvironmentVariable("wwwroot")
                      ?? TryJsonFile(userConfig, Wwwroot)
                      ?? "wwwroot";
        }

        public static string Wwwroot { get; set; }

        private static string TryJsonFile(string path, string key)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                using (var stream = File.OpenText(path))
                {
                    var json = stream.ReadToEnd();
                    var jobj = JObject.Parse(json);
                    if (jobj.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out var value))
                    {
                        return value.ToString();
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static string TryCommandLineArgs(string[] args, string key)
        {
            key = $"--{key}";
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].StartsWith(key, StringComparison.OrdinalIgnoreCase))
                    {
                        if (args[i].Contains("="))
                        {
                            var parts = args[i].Split(new[] {'='}, 2);
                            return parts[1];
                        }
                        else
                        {
                            if (args.Length > i + 1)
                            {
                                return args[i + 1];
                            }
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static string TryEnvironmentVariable(string key)
        {
            try
            {
                return Environment.GetEnvironmentVariable($"UNPKG_{key}");
            }
            catch
            {
                return null;
            }
        }
    }
}