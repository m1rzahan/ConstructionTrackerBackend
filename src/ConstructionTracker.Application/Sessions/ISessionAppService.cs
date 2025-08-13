using Abp.Application.Services;
using ConstructionTracker.Sessions.Dto;
using System.Threading.Tasks;

namespace ConstructionTracker.Sessions;

public interface ISessionAppService : IApplicationService
{
    Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
}
