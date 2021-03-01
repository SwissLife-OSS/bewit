using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureAppConfiguration(SetupConfiguration)
                        .UseStartup<Startup>();
                });

        private static void SetupConfiguration(
            WebHostBuilderContext builderContext,
            IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile($"appsettings.json",
                optional: true, reloadOnChange: true);
        }
    }
}
