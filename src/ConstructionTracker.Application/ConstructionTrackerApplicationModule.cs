using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using ConstructionTracker.Authorization;

namespace ConstructionTracker;

[DependsOn(
    typeof(ConstructionTrackerCoreModule),
    typeof(AbpAutoMapperModule))]
public class ConstructionTrackerApplicationModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.Authorization.Providers.Add<ConstructionTrackerAuthorizationProvider>();
    }

    public override void Initialize()
    {
        var thisAssembly = typeof(ConstructionTrackerApplicationModule).GetAssembly();

        IocManager.RegisterAssemblyByConvention(thisAssembly);

        Configuration.Modules.AbpAutoMapper().Configurators.Add(
            // Scan the assembly for classes which inherit from AutoMapper.Profile
            cfg => cfg.AddMaps(thisAssembly)
        );
    }
}
