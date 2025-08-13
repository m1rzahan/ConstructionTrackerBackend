using Abp.Modules;
using Abp.Reflection.Extensions;
using ConstructionTracker.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace ConstructionTracker.Web.Host.Startup
{
    [DependsOn(
       typeof(ConstructionTrackerWebCoreModule))]
    public class ConstructionTrackerWebHostModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public ConstructionTrackerWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(ConstructionTrackerWebHostModule).GetAssembly());
        }
    }
}
