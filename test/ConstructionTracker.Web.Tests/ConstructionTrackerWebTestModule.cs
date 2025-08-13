using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using ConstructionTracker.EntityFrameworkCore;
using ConstructionTracker.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace ConstructionTracker.Web.Tests;

[DependsOn(
    typeof(ConstructionTrackerWebMvcModule),
    typeof(AbpAspNetCoreTestBaseModule)
)]
public class ConstructionTrackerWebTestModule : AbpModule
{
    public ConstructionTrackerWebTestModule(ConstructionTrackerEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
    }

    public override void PreInitialize()
    {
        Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(ConstructionTrackerWebTestModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        IocManager.Resolve<ApplicationPartManager>()
            .AddApplicationPartsIfNotAddedBefore(typeof(ConstructionTrackerWebMvcModule).Assembly);
    }
}