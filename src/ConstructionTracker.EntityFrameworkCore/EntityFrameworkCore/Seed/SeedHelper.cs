using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Uow;
using Abp.MultiTenancy;
using ConstructionTracker.EntityFrameworkCore.Seed.Host;
using ConstructionTracker.EntityFrameworkCore.Seed.Tenants;
using ConstructionTracker.EntityFrameworkCore.Seed.ConstructionTracker;
using Microsoft.EntityFrameworkCore;
using System;
using System.Transactions;

namespace ConstructionTracker.EntityFrameworkCore.Seed;

public static class SeedHelper
{
    public static void SeedHostDb(IIocResolver iocResolver)
    {
        WithDbContext<ConstructionTrackerDbContext>(iocResolver, SeedHostDb);
    }

    public static void SeedHostDb(ConstructionTrackerDbContext context)
    {
        context.SuppressAutoSetTenantId = true;

        // Host seed
        new InitialHostDbBuilder(context).Create();

        // Default tenant seed (in host database).
        new DefaultTenantBuilder(context).Create();
        new TenantRoleAndUserBuilder(context, 1).Create();
        
        // Construction Tracker specific data
        new ConstructionTrackerDataBuilder(context).Create();
    }

    private static void WithDbContext<TDbContext>(IIocResolver iocResolver, Action<TDbContext> contextAction)
        where TDbContext : DbContext
    {
        using (var uowManager = iocResolver.ResolveAsDisposable<IUnitOfWorkManager>())
        {
            using (var uow = uowManager.Object.Begin(TransactionScopeOption.Suppress))
            {
                var context = uowManager.Object.Current.GetDbContext<TDbContext>(MultiTenancySides.Host);

                contextAction(context);

                uow.Complete();
            }
        }
    }
}
