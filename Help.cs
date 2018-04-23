using System;

namespace dotnet_unpkg
{
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
            Console.WriteLine("    dotnet unpkg add @aspnet/signalr/browser");
            Console.WriteLine();
        }

        public static void Restore()
        {
            Console.WriteLine("Usage: dotnet unpkg restore");
            Console.WriteLine();
        }
    }
}