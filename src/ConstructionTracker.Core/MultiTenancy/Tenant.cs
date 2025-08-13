using Abp.MultiTenancy;
using ConstructionTracker.Authorization.Users;

namespace ConstructionTracker.MultiTenancy;

public class Tenant : AbpTenant<User>
{
    public Tenant()
    {
    }

    public Tenant(string tenancyName, string name)
        : base(tenancyName, name)
    {
    }
}
