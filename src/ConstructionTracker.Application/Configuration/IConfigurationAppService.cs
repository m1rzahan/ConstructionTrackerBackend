using ConstructionTracker.Configuration.Dto;
using System.Threading.Tasks;

namespace ConstructionTracker.Configuration;

public interface IConfigurationAppService
{
    Task ChangeUiTheme(ChangeUiThemeInput input);
}
