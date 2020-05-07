using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Hosting;

namespace WindowsService1
{
    public class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            logger.Info($"Main, {string.Join(" ", args ?? new string[0])}");

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseNLog()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IDITestClass, DITestClass>();
                    services.AddSingleton<IDITestClass2, DITestClass2>();

                    logger.Info($"Add Worker");
                    services.AddHostedService<Worker>();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    logger.Info($"Add WebHost Startup");
                    webBuilder.UseUrls("http://localhost:51232");
                    webBuilder.UseStartup<Startup>();
                })
                .UseWindowsService();
    }
}
