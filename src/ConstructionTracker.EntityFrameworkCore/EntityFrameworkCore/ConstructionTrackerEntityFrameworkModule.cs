using Abp.EntityFrameworkCore.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Zero.EntityFrameworkCore;
using ConstructionTracker.EntityFrameworkCore.Seed;

namespace ConstructionTracker.EntityFrameworkCore;

[DependsOn(
    typeof(ConstructionTrackerCoreModule),
    typeof(AbpZeroCoreEntityFrameworkCoreModule))]
public class ConstructionTrackerEntityFrameworkModule : AbpModule
{
    /* Used it tests to skip dbcontext registration, in order to use in-memory database of EF Core */
    public bool SkipDbContextRegistration { get; set; }

    public bool SkipDbSeed { get; set; }

    public override void PreInitialize()
    {
        if (!SkipDbContextRegistration)
        {
            Configuration.Modules.AbpEfCore().AddDbContext<ConstructionTrackerDbContext>(options =>
            {
                if (options.ExistingConnection != null)
                {
                    ConstructionTrackerDbContextConfigurer.Configure(options.DbContextOptions, options.ExistingConnection);
                }
                else
                {
                    ConstructionTrackerDbContextConfigurer.Configure(options.DbContextOptions, options.ConnectionString);
                }
            });
        }
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(ConstructionTrackerEntityFrameworkModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        if (!SkipDbSeed)
        {
            SeedHelper.SeedHostDb(IocManager);
        }
    }

    public override void OnApplicationInitialization()
    {
        // Auto-migrate database
        using (var scope = IocManager.CreateScope())
        {
            var dbContext = scope.Resolve<ConstructionTrackerDbContext>();
            dbContext.Database.EnsureCreated();
        }
    }
}
