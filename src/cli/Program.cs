using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RendleLabs.Unpkg;

namespace dotnet_unpkg
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Help.Empty();
                return;
            }
            
            Settings.Initialize(Flags(args).ToArray());

            try
            {
            switch (args[0])
            {
                case "--help":
                case "-h":
                    Help.Empty();
                    return;
                case "--add-task":
                    AddBuildTask();
                    return;
                case "a":
                case "add":
                    await Add.Run(CommandArguments(args));
                    break;
                case "u":
                case "update":
                case "upgrade":
                case "up":
                    await Upgrade.Run(CommandArguments(args));
                    break;
                case "r":
                case "restore":
                    var results = await Restore.Run(CommandArguments(args));
                    WriteRestoreResults(results);
                    break;
                default:
                    Help.Empty();
                    break;
            }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private static void AddBuildTask()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "add package RendleLabs.Unpkg.Build",
                UseShellExecute = false,
                LoadUserProfile = true
            };
            Process.Start(psi)?.WaitForExit();
        }

        private static void WriteRestoreResults(RestoreResults results)
        {
            if (!string.IsNullOrEmpty(results.Message))
            {
                Console.WriteLine(results.Message);
            }
            else
            {
                foreach (var result in results.Results)
                {
                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        Console.WriteLine(result.Message);
                    }
                    else
                    {
                        Console.WriteLine($"{result.CdnUrl} -> {result.LocalFile}");
                    }
                }
            }
        }

        private static IEnumerable<string> Flags(string[] args)
        {
            for (int i = 1, l = args.Length; i < l; i++)
            {
                if (args[i].StartsWith('-') || args[i].StartsWith('/'))
                {
                    yield return args[i];

                    if (!args[i].Contains('='))
                    {
                        if (++i >= args.Length)
                        {
                            break;
                        }

                        yield return args[i];
                    }
                }
            }
        }

        private static IEnumerable<string> CommandArguments(string[] args)
        {
            for (int i = 1, l = args.Length; i < l; i++)
            {
                if (args[i].StartsWith('-'))
                {
                    // --wwwroot=public
                    if (args[i].Contains('=')) continue;
                    
                    // --wwwroot public
                    i++;
                    continue;
                }

                yield return args[i];
            }
        }
    }
}
