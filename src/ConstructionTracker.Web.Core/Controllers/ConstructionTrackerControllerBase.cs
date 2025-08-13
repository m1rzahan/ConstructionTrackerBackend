using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace ConstructionTracker.Controllers
{
    public abstract class ConstructionTrackerControllerBase : AbpController
    {
        protected ConstructionTrackerControllerBase()
        {
            LocalizationSourceName = ConstructionTrackerConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
