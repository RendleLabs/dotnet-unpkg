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
                    ["version"] = CleanVersion(entry.Version),
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

        public static string CleanPackageName(string full) => full.Split('@').FirstOrDefault();

        public static string CleanVersion(string full) => full.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
    }

    public static class Help
    {
        public static void Empty()
        {
            Console.WriteLine("Usage: dotnet unpkg [command] [arguments]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  add            Add a package");
            Console.WriteLine("  restore        Restore packages");
        }

        public static void Add()
        {
            Console.WriteLine("Usage: dotnet unpkg add <PACKAGE> [...<PACKAGE>]");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  <PACKAGE>    The name of a package on unpkg.com.");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("    dotnet unpkg add jquery bootstrap popper.js");
            Console.WriteLine("    dotnet unpkg add bootswatch/yeti");
            Console.WriteLine();
        }

        public static void Restore()
        {
            Console.WriteLine("Usage: dotnet unpkg restore");
            Console.WriteLine();
        }
    }
}