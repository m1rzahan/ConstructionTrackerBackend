using Abp.AspNetCore.Dependency;
using Abp.Dependency;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;

namespace ConstructionTracker.Web.Host.Startup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        internal static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    
                    // Railway için port ayarları
                    var port = Environment.GetEnvironmentVariable("PORT") ?? "44311";
                    webBuilder.UseUrls($"http://0.0.0.0:{port}");
                })
                .UseCastleWindsor(IocManager.Instance.IocContainer);
    }
}
