using Abp.AspNetCore.Mvc.Controllers;
using Abp.Authorization;
using ConstructionTracker.ConstructionTracker.Dto;
using ConstructionTracker.ConstructionTracker.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ConstructionTracker.Web.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConstructionTrackerController : AbpController
    {
        private readonly IConstructionTrackerAppService _constructionTrackerAppService;

        public ConstructionTrackerController(IConstructionTrackerAppService constructionTrackerAppService)
        {
            _constructionTrackerAppService = constructionTrackerAppService;
        }

        /// <summary>
        /// Android uygulaması için kullanıcı girişi
        /// </summary>
        [HttpPost("login")]
        public async Task<UserLoginResultDto> Login([FromBody] UserLoginDto input)
        {
            return await _constructionTrackerAppService.LoginAsync(input);
        }

        /// <summary>
        /// Kullanıcı çıkışı
        /// </summary>
        [HttpPost("logout/{userId}")]
        public async Task<IActionResult> Logout(long userId)
        {
            await _constructionTrackerAppService.LogoutAsync(userId);
            return Ok();
        }

        /// <summary>
        /// Dashboard verilerini getir (Android uygulaması ana ekranı için)
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<DashboardDto> GetDashboardData()
        {
            return await _constructionTrackerAppService.GetDashboardDataAsync();
        }

        /// <summary>
        /// Haftalık istatistikleri getir
        /// </summary>
        [HttpGet("weekly-stats")]
        public async Task<WeeklyStatsDto> GetWeeklyStats()
        {
            return await _constructionTrackerAppService.GetWeeklyStatsAsync();
        }

        /// <summary>
        /// Mevcut kullanıcı bilgilerini getir
        /// </summary>
        [HttpGet("current-user")]
        public async Task<ConstructionTrackerUserDto> GetCurrentUser()
        {
            return await _constructionTrackerAppService.GetCurrentUserAsync();
        }

        /// <summary>
        /// Mevcut kullanıcı bilgilerini güncelle
        /// </summary>
        [HttpPut("current-user")]
        public async Task<ConstructionTrackerUserDto> UpdateCurrentUser([FromBody] UpdateUserDto input)
        {
            return await _constructionTrackerAppService.UpdateCurrentUserAsync(input);
        }
    }
} 