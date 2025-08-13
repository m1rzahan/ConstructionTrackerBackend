using Abp.Application.Services;
using ConstructionTracker.ConstructionTracker.Dto;
using System;
using System.Threading.Tasks;

namespace ConstructionTracker.ConstructionTracker.Services
{
    public interface IConstructionTrackerAppService : IApplicationService
    {
        // Authentication
        Task<UserLoginResultDto> LoginAsync(UserLoginDto input);
        Task LogoutAsync(long userId);

        // Dashboard
        Task<DashboardDto> GetDashboardDataAsync();
        Task<WeeklyStatsDto> GetWeeklyStatsAsync();

        // User Management (for authenticated user)
        Task<ConstructionTrackerUserDto> GetCurrentUserAsync();
        Task<ConstructionTrackerUserDto> UpdateCurrentUserAsync(UpdateUserDto input);
    }
} 