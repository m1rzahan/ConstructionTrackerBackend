using Abp.Authorization;
using Abp.Runtime.Session;
using ConstructionTracker.Configuration.Dto;
using System.Threading.Tasks;

namespace ConstructionTracker.Configuration;

[AbpAuthorize]
public class ConfigurationAppService : ConstructionTrackerAppServiceBase, IConfigurationAppService
{
    public async Task ChangeUiTheme(ChangeUiThemeInput input)
    {
        await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
    }
}
