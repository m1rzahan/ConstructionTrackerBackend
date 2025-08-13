using Abp.Authorization;
using ConstructionTracker.Authorization.Roles;
using ConstructionTracker.Authorization.Users;

namespace ConstructionTracker.Authorization;

public class PermissionChecker : PermissionChecker<Role, User>
{
    public PermissionChecker(UserManager userManager)
        : base(userManager)
    {
    }
}
