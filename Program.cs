using System;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_unpkg
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: unpkg add [package]");
            }

            if (args[0] == "add")
            {
                await Add.Run(args.Skip(1));
            }
        }
    }
}
