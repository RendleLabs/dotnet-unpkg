using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            
            Settings.Initialize(args);

            try
            {
            switch (args[0])
            {
                case "--help":
                case "-h":
                    Help.Empty();
                    return;
                case "a":
                case "add":
                    await Add.Run(CleanArguments(args));
                    break;
                case "u":
                case "update":
                case "upgrade":
                case "up":
                    await Upgrade.Run(args.Skip(1));
                    break;
                case "r":
                case "restore":
                    await Restore.Run(CleanArguments(args));
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

        private static IEnumerable<string> CleanArguments(string[] args)
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
