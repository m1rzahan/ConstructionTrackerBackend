using Abp.Application.Services;
using ConstructionTracker.Authorization.Accounts.Dto;
using System.Threading.Tasks;

namespace ConstructionTracker.Authorization.Accounts;

public interface IAccountAppService : IApplicationService
{
    Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

    Task<RegisterOutput> Register(RegisterInput input);
}
