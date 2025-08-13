using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using ConstructionTracker.Configuration;
using ConstructionTracker.EntityFrameworkCore;
using ConstructionTracker.Migrator.DependencyInjection;
using Castle.MicroKernel.Registration;
using Microsoft.Extensions.Configuration;

namespace ConstructionTracker.Migrator;

[DependsOn(typeof(ConstructionTrackerEntityFrameworkModule))]
public class ConstructionTrackerMigratorModule : AbpModule
{
    private readonly IConfigurationRoot _appConfiguration;

    public ConstructionTrackerMigratorModule(ConstructionTrackerEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbSeed = true;

        _appConfiguration = AppConfigurations.Get(
            typeof(ConstructionTrackerMigratorModule).GetAssembly().GetDirectoryPathOrNull()
        );
    }

    public override void PreInitialize()
    {
        Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
            ConstructionTrackerConsts.ConnectionStringName
        );

        Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
        Configuration.ReplaceService(
            typeof(IEventBus),
            () => IocManager.IocContainer.Register(
                Component.For<IEventBus>().Instance(NullEventBus.Instance)
            )
        );
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(ConstructionTrackerMigratorModule).GetAssembly());
        ServiceCollectionRegistrar.Register(IocManager);
    }
}
