using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.ObjectMapping;
using Abp.Runtime.Session;
using ConstructionTracker.Authorization.Users;
using ConstructionTracker.ConstructionTracker.Dto;
using ConstructionTracker.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ConstructionTracker.ConstructionTracker.Services
{
    [AbpAuthorize]
    public class ConstructionTrackerAppService : ConstructionTrackerAppServiceBase, IConstructionTrackerAppService
    {
        private readonly UserManager _userManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Company, Guid> _companyRepository;
        private readonly IRepository<Project, Guid> _projectRepository;
        private readonly IRepository<ActivityLog, Guid> _activityLogRepository;
        private readonly IRepository<QrCodeScan, Guid> _qrCodeScanRepository;
        private readonly IRepository<UserProject, Guid> _userProjectRepository;
        private readonly IConfiguration _configuration;

        public ConstructionTrackerAppService(
            UserManager userManager,
            IRepository<User, long> userRepository,
            IRepository<Company, Guid> companyRepository,
            IRepository<Project, Guid> projectRepository,
            IRepository<ActivityLog, Guid> activityLogRepository,
            IRepository<QrCodeScan, Guid> qrCodeScanRepository,
            IRepository<UserProject, Guid> userProjectRepository,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _projectRepository = projectRepository;
            _activityLogRepository = activityLogRepository;
            _qrCodeScanRepository = qrCodeScanRepository;
            _userProjectRepository = userProjectRepository;
            _configuration = configuration;
        }

        [AbpAllowAnonymous]
        public async Task<UserLoginResultDto> LoginAsync(UserLoginDto input)
        {
            var user = await _userManager.FindByEmailAsync(input.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, input.Password))
            {
                throw new AbpAuthorizationException("Invalid email or password");
            }

            if (!user.IsActive)
            {
                throw new AbpAuthorizationException("User account is inactive");
            }

            // Update last login time
            user.LastLoginAt = DateTime.Now;
            await _userRepository.UpdateAsync(user);

            // Log activity
            await LogActivityAsync(new CreateActivityLogDto
            {
                Title = "Kullanıcı Girişi",
                Description = $"{user.FirstName} {user.LastName} sisteme giriş yaptı",
                ActivityType = ActivityType.UserLogin,
                UserId = user.Id,
                CompanyId = user.CompanyId,
                ActivityDate = DateTime.Now
            });

            var userDto = ObjectMapper.Map<ConstructionTrackerUserDto>(user);
            
            // Get company name if available
            if (user.CompanyId.HasValue)
            {
                var company = await _companyRepository.FirstOrDefaultAsync(user.CompanyId.Value);
                userDto.CompanyName = company?.Name;
            }

            return new UserLoginResultDto
            {
                User = userDto,
                Token = GenerateToken(user),
                RefreshToken = GenerateRefreshToken(user)
            };
        }

        public async Task LogoutAsync(long userId)
        {
            var user = await _userRepository.GetAsync(userId);
            
            // Log activity
            await LogActivityAsync(new CreateActivityLogDto
            {
                Title = "Kullanıcı Çıkışı",
                Description = $"{user.FirstName} {user.LastName} sistemden çıkış yaptı",
                ActivityType = ActivityType.UserLogout,
                UserId = user.Id,
                CompanyId = user.CompanyId,
                ActivityDate = DateTime.Now
            });
        }

        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            var currentUser = await GetCurrentUserInternalAsync();
            var companyId = currentUser.CompanyId;

            // Get statistics
            var stats = await GetDashboardStatsAsync(companyId);
            
            // Get recent projects
            var recentProjects = await GetRecentProjectsAsync(companyId, currentUser.Id);
            
            // Get recent activities
            var recentActivities = await GetRecentActivitiesAsync(companyId);

            return new DashboardDto
            {
                Stats = stats,
                RecentProjects = recentProjects,
                RecentActivities = recentActivities,
                CurrentUser = ObjectMapper.Map<ConstructionTrackerUserDto>(currentUser)
            };
        }

        public async Task<WeeklyStatsDto> GetWeeklyStatsAsync()
        {
            var currentUser = await GetCurrentUserInternalAsync();
            var companyId = currentUser.CompanyId;
            
            var startDate = DateTime.Today.AddDays(-7);
            var endDate = DateTime.Today;

            var dailyStats = new List<DailyStatDto>();
            
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var checkinCount = await _qrCodeScanRepository.CountAsync(x => 
                    x.ScanDate.Date == date && 
                    x.ScanType == ScanType.CheckIn &&
                    (companyId == null || x.User.CompanyId == companyId));
                
                var activeUsers = await _qrCodeScanRepository
                    .GetAll()
                    .Where(x => x.ScanDate.Date == date && (companyId == null || x.User.CompanyId == companyId))
                    .Select(x => x.UserId)
                    .Distinct()
                    .CountAsync();

                dailyStats.Add(new DailyStatDto
                {
                    Date = date,
                    CheckinCount = checkinCount,
                    ActiveUsers = activeUsers,
                    DayName = date.ToString("dddd", new System.Globalization.CultureInfo("tr-TR"))
                });
            }

            var totalCheckins = await _qrCodeScanRepository.CountAsync(x => 
                x.ScanDate >= startDate && 
                x.ScanDate <= endDate &&
                x.ScanType == ScanType.CheckIn &&
                (companyId == null || x.User.CompanyId == companyId));

            var totalUsers = await _userRepository.CountAsync(x => 
                x.IsActive && 
                (companyId == null || x.CompanyId == companyId));

            var activeProjects = await _projectRepository.CountAsync(x => 
                x.IsActive && 
                x.Status == ProjectStatus.Active &&
                (companyId == null || x.CompanyId == companyId));

            return new WeeklyStatsDto
            {
                DailyStats = dailyStats,
                TotalCheckins = totalCheckins,
                TotalUsers = totalUsers,
                ActiveProjects = activeProjects
            };
        }

        public async Task<ConstructionTrackerUserDto> GetCurrentUserAsync()
        {
            var user = await GetCurrentUserInternalAsync();
            var userDto = ObjectMapper.Map<ConstructionTrackerUserDto>(user);
            
            if (user.CompanyId.HasValue)
            {
                var company = await _companyRepository.FirstOrDefaultAsync(user.CompanyId.Value);
                userDto.CompanyName = company?.Name;
            }

            return userDto;
        }

        public async Task<ConstructionTrackerUserDto> UpdateCurrentUserAsync(UpdateUserDto input)
        {
            var user = await GetCurrentUserInternalAsync();
            
            user.EmailAddress = input.Email;
            user.FirstName = input.FirstName;
            user.LastName = input.LastName;
            user.Role = input.Role;
            user.CompanyId = input.CompanyId;
            user.IsActive = input.IsActive;

            await _userRepository.UpdateAsync(user);

            return ObjectMapper.Map<ConstructionTrackerUserDto>(user);
        }

        #region Private Methods

        private async Task<User> GetCurrentUserInternalAsync()
        {
            var userId = AbpSession.GetUserId();
            return await _userRepository
                .GetAllIncluding(x => x.Company)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }

        private async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid? companyId)
        {
            var activeProjects = await _projectRepository.CountAsync(x => 
                x.IsActive && 
                x.Status == ProjectStatus.Active &&
                (companyId == null || x.CompanyId == companyId));

            var totalPersonnel = await _userRepository.CountAsync(x => 
                x.IsActive && 
                (companyId == null || x.CompanyId == companyId));

            var monthlyCheckins = await _qrCodeScanRepository.CountAsync(x => 
                x.ScanDate >= DateTime.Today.AddDays(-30) && 
                x.ScanType == ScanType.CheckIn &&
                (companyId == null || x.User.CompanyId == companyId));

            var pendingTasks = await _activityLogRepository.CountAsync(x => 
                !x.IsRead && 
                x.Priority >= ActivityPriority.High &&
                (companyId == null || x.CompanyId == companyId));

            return new DashboardStatsDto
            {
                ActiveProjects = activeProjects,
                TotalPersonnel = totalPersonnel,
                MonthlyCheckins = monthlyCheckins,
                PendingTasks = pendingTasks,
                ActiveProjectsTrend = "+2",
                TotalPersonnelTrend = "+5",
                MonthlyCheckinsTrend = "+12%",
                PendingTasksTrend = "-3"
            };
        }

        private async Task<List<ProjectSummaryDto>> GetRecentProjectsAsync(Guid? companyId, long userId)
        {
            var query = _projectRepository.GetAll()
                .Where(x => x.IsActive && (companyId == null || x.CompanyId == companyId));

            // For non-admin users, filter by assigned projects
            var currentUser = await _userRepository.GetAsync(userId);
            if (currentUser.Role != UserRoleType.Admin)
            {
                var userProjectIds = await _userProjectRepository.GetAll()
                    .Where(x => x.UserId == userId && x.IsActive)
                    .Select(x => x.ProjectId)
                    .ToListAsync();
                
                query = query.Where(x => userProjectIds.Contains(x.Id));
            }

            var projects = await query
                .OrderByDescending(x => x.CreationTime)
                .Take(3)
                .ToListAsync();

            return ObjectMapper.Map<List<ProjectSummaryDto>>(projects);
        }

        private async Task<List<DashboardActivityDto>> GetRecentActivitiesAsync(Guid? companyId)
        {
            var activities = await _activityLogRepository.GetAll()
                .Include(x => x.User)
                .Where(x => companyId == null || x.CompanyId == companyId)
                .OrderByDescending(x => x.ActivityDate)
                .Take(5)
                .ToListAsync();

            return activities.Select(x => new DashboardActivityDto
            {
                Title = x.Title,
                Description = x.Description,
                TimeAgo = GetTimeAgo(x.ActivityDate),
                Icon = GetActivityIcon(x.ActivityType),
                Color = GetActivityColor(x.Priority)
            }).ToList();
        }

        private async Task LogActivityAsync(CreateActivityLogDto input)
        {
            var activity = ObjectMapper.Map<ActivityLog>(input);
            await _activityLogRepository.InsertAsync(activity);
        }

        private string GenerateToken(User user)
        {
            // JWT configuration values from appsettings.json
            var securityKey = _configuration["Authentication:JwtBearer:SecurityKey"] ?? "ConstructionTracker_8F793AE2B81E4211968F75924FBEF66E";
            var issuer = _configuration["Authentication:JwtBearer:Issuer"] ?? "ConstructionTracker";
            var audience = _configuration["Authentication:JwtBearer:Audience"] ?? "ConstructionTracker";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.EmailAddress),
                new Claim(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
                new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
                new Claim("role", user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Add company claim if user has a company
            if (user.CompanyId.HasValue)
            {
                claims.Add(new Claim("companyId", user.CompanyId.Value.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(securityKey));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,
                expires: now.AddDays(1), // 1 day expiration
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        private string GenerateRefreshToken(User user)
        {
            // Generate a simple refresh token (in production, store this in database with expiration)
            return $"refresh_token_{user.Id}_{Guid.NewGuid()}";
        }

        private string GetTimeAgo(DateTime activityDate)
        {
            var timeSpan = DateTime.Now - activityDate;
            
            if (timeSpan.TotalMinutes < 1)
                return "Az önce";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} dakika önce";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} saat önce";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} gün önce";
            
            return activityDate.ToString("dd.MM.yyyy");
        }

        private string GetActivityIcon(ActivityType activityType)
        {
            return activityType switch
            {
                ActivityType.UserLogin => "account-check",
                ActivityType.UserLogout => "account-minus",
                ActivityType.ProjectCreated => "file-plus",
                ActivityType.ProjectUpdated => "file-edit",
                ActivityType.ProjectCompleted => "check-circle",
                ActivityType.QrCodeScanned => "qrcode-scan",
                ActivityType.ReportGenerated => "chart-line",
                ActivityType.MaterialAdded => "package-variant",
                ActivityType.MaterialUsed => "package-variant-closed",
                ActivityType.PersonnelAssigned => "account-plus",
                ActivityType.PersonnelRemoved => "account-minus",
                _ => "information"
            };
        }

        private string GetActivityColor(ActivityPriority priority)
        {
            return priority switch
            {
                ActivityPriority.Low => "#4CAF50",
                ActivityPriority.Normal => "#2196F3",
                ActivityPriority.High => "#FF9800",
                ActivityPriority.Critical => "#F44336",
                _ => "#2196F3"
            };
        }

        #endregion
    }
} 