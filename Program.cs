using System;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_unpkg
{
    public static class Program
    {
        static async Task Main(string[] args)
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
                case "add":
                    await Add.Run(args.Skip(1));
                    break;
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
