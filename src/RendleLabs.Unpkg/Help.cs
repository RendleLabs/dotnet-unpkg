using System;

namespace RendleLabs.Unpkg
{
    public static class Help
    {
        public static void Empty()
        {
            Console.WriteLine("Usage: unpkg [command] [options] [arguments]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  add            Add a package");
            Console.WriteLine("  restore        Restore packages");
            Console.WriteLine("  upgrade        Upgrade packages");
            Console.WriteLine("  --add-task     Install the RendleLabs.Unpkg.Build package to restore as part of build");
        }

        public static void Add()
        {
            Console.WriteLine("Usage: unpkg add [OPTIONS] <PACKAGE> [...<PACKAGE>]");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  <PACKAGE>    The name of a package on unpkg.com.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --wwwroot    Name of your static files directory");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("    unpkg add --wwwroot=public jquery bootstrap popper.js");
            Console.WriteLine("    unpkg add bootswatch/yeti");
            Console.WriteLine("    unpkg add @aspnet/signalr/browser");
            Console.WriteLine();
        }

        public static void Restore()
        {
            Console.WriteLine("Usage: unpkg restore");
            Console.WriteLine();
        }

        public static void Upgrade()
        {
            Console.WriteLine("Usage: unpkg upgrade [<PACKAGE> [...<PACKAGE>]]");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  <PACKAGE>    (Optional) The name of an installed package.");
            Console.WriteLine("               If omitted, all packages will be upgraded.");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("    unpkg upgrade");
            Console.WriteLine("    unpkg upgrade jquery bootstrap popper.js");
            Console.WriteLine("    unpkg upgrade @aspnet/signalr/browser");
            Console.WriteLine();
        }
    }
}