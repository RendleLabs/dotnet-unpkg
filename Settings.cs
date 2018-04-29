using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace dotnet_unpkg
{
    public static class Settings
    {
        public static IConfiguration Configuration { get; private set; }
        
        public static void Initialize(string[] args)
        {
            var localConfig = Path.Combine(Environment.CurrentDirectory, "unpkg.config");
            var userConfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".unpkg", "unpkg.config");

            Configuration = new ConfigurationBuilder()
                .AddJsonFile(userConfig, true)
                .AddEnvironmentVariables("UNPKG_")
                .AddJsonFile(localConfig, true)
                .AddCommandLine(args)
                .Build();

        }

        public static string Wwwroot => Configuration["wwwroot"] ?? "wwwroot";
    }
}