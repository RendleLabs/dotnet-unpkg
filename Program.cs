using System;
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
                    await Add.Run(args.Skip(1));
                    break;
                case "u":
                case "update":
                case "upgrade":
                case "up":
                    await Upgrade.Run(args.Skip(1));
                    break;
                case "r":
                case "restore":
                    await Restore.Run(args.Skip(1));
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
    }
}
