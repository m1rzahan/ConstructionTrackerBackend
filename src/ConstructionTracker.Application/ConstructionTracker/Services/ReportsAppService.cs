using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.ObjectMapping;
using Abp.Runtime.Session;
using ConstructionTracker.Authorization.Users;
using ConstructionTracker.ConstructionTracker.Dto;
using ConstructionTracker.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ConstructionTracker.ConstructionTracker.Services
{
    [AbpAuthorize]
    public class ReportsAppService : ConstructionTrackerAppServiceBase, IReportsAppService
    {
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Company, Guid> _companyRepository;
        private readonly IRepository<Project, Guid> _projectRepository;
        private readonly IRepository<UserProject, Guid> _userProjectRepository;
        private readonly IRepository<QrCodeScan, Guid> _qrCodeScanRepository;
        private readonly IRepository<ActivityLog, Guid> _activityLogRepository;

        public ReportsAppService(
            IRepository<User, long> userRepository,
            IRepository<Company, Guid> companyRepository,
            IRepository<Project, Guid> projectRepository,
            IRepository<UserProject, Guid> userProjectRepository,
            IRepository<QrCodeScan, Guid> qrCodeScanRepository,
            IRepository<ActivityLog, Guid> activityLogRepository)
        {
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _projectRepository = projectRepository;
            _userProjectRepository = userProjectRepository;
            _qrCodeScanRepository = qrCodeScanRepository;
            _activityLogRepository = activityLogRepository;
        }

        public async Task<DashboardReportDto> GetDashboardReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var currentUser = await GetCurrentUserAsync();
            var companyId = currentUser.CompanyId;

            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today.AddDays(1);

            // Get stats
            var stats = await GetDashboardStatsInternalAsync(companyId);

            // Get top projects
            var topProjects = await _projectRepository.GetAll()
                .Where(x => x.IsActive && (companyId == null || x.CompanyId == companyId))
                .OrderByDescending(x => x.Progress)
                .Take(5)
                .ToListAsync();

            // Get top personnel
            var topPersonnel = await _userRepository.GetAll()
                .Where(x => x.IsActive && (companyId == null || x.CompanyId == companyId))
                .OrderByDescending(x => x.LastLoginAt)
                .Take(5)
                .ToListAsync();

            // Get recent activities
            var recentActivities = await _activityLogRepository.GetAll()
                .Where(x => x.ActivityDate >= start && x.ActivityDate <= end &&
                           (companyId == null || x.CompanyId == companyId))
                .OrderByDescending(x => x.ActivityDate)
                .Take(10)
                .Select(x => new DailyActivityDto
                {
                    Date = x.ActivityDate,
                    Activity = x.Title,
                    User = x.UserName,
                    Project = x.ProjectName,
                    Type = x.ActivityType.ToString()
                })
                .ToListAsync();

            return new DashboardReportDto
            {
                ReportDate = DateTime.Now,
                Stats = stats,
                TopProjects = ObjectMapper.Map<List<ProjectSummaryDto>>(topProjects),
                TopPersonnel = ObjectMapper.Map<List<ConstructionTrackerUserDto>>(topPersonnel),
                RecentActivities = recentActivities
            };
        }

        public async Task<ProjectReportDto> GetProjectsReportAsync(Guid? companyId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var currentUser = await GetCurrentUserAsync();
            if (companyId == null && currentUser.Role != UserRoleType.Admin)
                companyId = currentUser.CompanyId;

            var query = _projectRepository.GetAll()
                .Where(x => companyId == null || x.CompanyId == companyId);

            var totalProjects = await query.CountAsync();
            var activeProjects = await query.CountAsync(x => x.Status == ProjectStatus.Active);
            var completedProjects = await query.CountAsync(x => x.Status == ProjectStatus.Completed);
            var delayedProjects = await query.CountAsync(x => x.Status == ProjectStatus.Delayed);

            var totalBudget = await query.SumAsync(x => x.Budget ?? 0);
            var totalSpent = await query.SumAsync(x => x.SpentAmount ?? 0);
            var averageProgress = await query.AverageAsync(x => (double?)x.Progress) ?? 0;

            var projects = await query.ToListAsync();

            var budgetBreakdown = projects
                .Where(x => x.Budget.HasValue)
                .Select(x => new ProjectBudgetDto
                {
                    ProjectId = x.Id,
                    ProjectName = x.Name,
                    Budget = x.Budget.Value,
                    Spent = x.SpentAmount ?? 0,
                    Remaining = x.Budget.Value - (x.SpentAmount ?? 0),
                    PercentageUsed = x.Budget.Value > 0 ? ((x.SpentAmount ?? 0) / x.Budget.Value) * 100 : 0
                })
                .ToList();

            return new ProjectReportDto
            {
                TotalProjects = totalProjects,
                ActiveProjects = activeProjects,
                CompletedProjects = completedProjects,
                DelayedProjects = delayedProjects,
                TotalBudget = totalBudget,
                TotalSpent = totalSpent,
                AverageProgress = (decimal)averageProgress,
                Projects = ObjectMapper.Map<List<ProjectSummaryDto>>(projects),
                BudgetBreakdown = budgetBreakdown
            };
        }

        public async Task<PersonnelReportDto> GetPersonnelReportAsync(Guid? companyId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var currentUser = await GetCurrentUserAsync();
            if (companyId == null && currentUser.Role != UserRoleType.Admin)
                companyId = currentUser.CompanyId;

            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today.AddDays(1);

            var query = _userRepository.GetAll()
                .Where(x => companyId == null || x.CompanyId == companyId);

            var totalPersonnel = await query.CountAsync();
            var activePersonnel = await query.CountAsync(x => x.IsActive);

            var totalCheckIns = await _qrCodeScanRepository.CountAsync(x =>
                x.ScanDate >= start && x.ScanDate <= end &&
                x.ScanType == ScanType.CheckIn &&
                (companyId == null || x.User.CompanyId == companyId));

            // Calculate average working hours (simplified)
            var averageWorkingHours = totalCheckIns > 0 ? (decimal)(totalCheckIns * 8.0 / Math.Max(1, activePersonnel)) : 0;

            var personnelPerformance = await query
                .Where(x => x.IsActive)
                .Select(x => new PersonnelPerformanceDto
                {
                    UserId = x.Id,
                    UserName = x.FirstName + " " + x.LastName,
                    WorkDays = _qrCodeScanRepository.GetAll()
                        .Where(s => s.UserId == x.Id && s.ScanDate >= start && s.ScanDate <= end)
                        .Select(s => s.ScanDate.Date)
                        .Distinct()
                        .Count(),
                    WorkingHours = _qrCodeScanRepository.GetAll()
                        .Where(s => s.UserId == x.Id && s.ScanDate >= start && s.ScanDate <= end && s.ScanType == ScanType.CheckIn)
                        .Count() * 8,
                    ProjectCount = _userProjectRepository.GetAll()
                        .Where(up => up.UserId == x.Id && up.IsActive)
                        .Count(),
                    PerformanceScore = 85 // Placeholder calculation
                })
                .ToListAsync();

            var departmentStats = await query
                .Where(x => x.IsActive)
                .GroupBy(x => x.Role)
                .Select(g => new DepartmentStatsDto
                {
                    Department = g.Key.ToString(),
                    PersonnelCount = g.Count(),
                    TotalWorkingHours = g.Sum(x => _qrCodeScanRepository.GetAll()
                        .Where(s => s.UserId == x.Id && s.ScanDate >= start && s.ScanDate <= end && s.ScanType == ScanType.CheckIn)
                        .Count() * 8),
                    AveragePerformance = 85 // Placeholder
                })
                .ToListAsync();

            return new PersonnelReportDto
            {
                TotalPersonnel = totalPersonnel,
                ActivePersonnel = activePersonnel,
                TotalCheckIns = totalCheckIns,
                AverageWorkingHours = averageWorkingHours,
                PersonnelPerformance = personnelPerformance,
                DepartmentStats = departmentStats
            };
        }

        public async Task<QrScanReportDto> GetQrScanReportAsync(Guid? projectId = null, long? userId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var currentUser = await GetCurrentUserAsync();
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today.AddDays(1);

            var query = _qrCodeScanRepository.GetAll()
                .Where(x => x.ScanDate >= start && x.ScanDate <= end);

            if (projectId.HasValue)
                query = query.Where(x => x.ProjectId == projectId);

            if (userId.HasValue)
                query = query.Where(x => x.UserId == userId);

            // Apply company filter for non-admin users
            if (currentUser.Role != UserRoleType.Admin && currentUser.CompanyId.HasValue)
                query = query.Where(x => x.User.CompanyId == currentUser.CompanyId);

            var totalScans = await query.CountAsync();
            var totalCheckIns = await query.CountAsync(x => x.ScanType == ScanType.CheckIn);
            var totalCheckOuts = await query.CountAsync(x => x.ScanType == ScanType.CheckOut);
            var uniqueUsers = await query.Select(x => x.UserId).Distinct().CountAsync();

            var recentScans = await query
                .OrderByDescending(x => x.ScanDate)
                .Take(20)
                .ToListAsync();

            var hourlyActivity = await query
                .GroupBy(x => x.ScanDate.Hour)
                .Select(g => new HourlyActivityDto
                {
                    Hour = g.Key,
                    CheckInCount = g.Count(x => x.ScanType == ScanType.CheckIn),
                    CheckOutCount = g.Count(x => x.ScanType == ScanType.CheckOut)
                })
                .OrderBy(x => x.Hour)
                .ToListAsync();

            return new QrScanReportDto
            {
                TotalScans = totalScans,
                TotalCheckIns = totalCheckIns,
                TotalCheckOuts = totalCheckOuts,
                UniqueUsers = uniqueUsers,
                RecentScans = ObjectMapper.Map<List<QrCodeScanDto>>(recentScans),
                HourlyActivity = hourlyActivity
            };
        }

        public async Task<DailyActivityReportDto> GetDailyActivityReportAsync(DateTime date)
        {
            var currentUser = await GetCurrentUserAsync();
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var query = _qrCodeScanRepository.GetAll()
                .Where(x => x.ScanDate >= startOfDay && x.ScanDate < endOfDay);

            // Apply company filter for non-admin users
            if (currentUser.Role != UserRoleType.Admin && currentUser.CompanyId.HasValue)
                query = query.Where(x => x.User.CompanyId == currentUser.CompanyId);

            var totalCheckIns = await query.CountAsync(x => x.ScanType == ScanType.CheckIn);
            var totalCheckOuts = await query.CountAsync(x => x.ScanType == ScanType.CheckOut);
            var activeUsers = await query.Select(x => x.UserId).Distinct().CountAsync();

            var activeProjects = await query.Select(x => x.ProjectId).Distinct().CountAsync();

            var activities = await query.OrderBy(x => x.ScanDate).ToListAsync();

            return new DailyActivityReportDto
            {
                Date = date,
                TotalCheckIns = totalCheckIns,
                TotalCheckOuts = totalCheckOuts,
                ActiveUsers = activeUsers,
                ActiveProjects = activeProjects,
                Activities = ObjectMapper.Map<List<QrCodeScanDto>>(activities)
            };
        }

        public async Task<WeeklyReportDto> GetWeeklyReportAsync(DateTime? weekStart = null)
        {
            var start = weekStart ?? DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var end = start.AddDays(7);

            var dailyStats = new List<DailyStatDto>();
            for (var date = start; date < end; date = date.AddDays(1))
            {
                var dailyReport = await GetDailyActivityReportAsync(date);
                dailyStats.Add(new DailyStatDto
                {
                    Date = date,
                    CheckinCount = dailyReport.TotalCheckIns,
                    ActiveUsers = dailyReport.ActiveUsers,
                    DayName = date.ToString("dddd", new CultureInfo("tr-TR"))
                });
            }

            var summary = new WeeklySummaryDto
            {
                TotalWorkDays = dailyStats.Count(x => x.CheckinCount > 0),
                TotalCheckIns = dailyStats.Sum(x => x.CheckinCount),
                TotalActiveUsers = dailyStats.Max(x => x.ActiveUsers),
                TotalActiveProjects = await _projectRepository.CountAsync(x => x.IsActive),
                AverageWorkingHours = dailyStats.Sum(x => x.CheckinCount * 8) / Math.Max(1, dailyStats.Sum(x => x.ActiveUsers))
            };

            return new WeeklyReportDto
            {
                WeekStart = start,
                WeekEnd = end.AddDays(-1),
                DailyStats = dailyStats,
                Summary = summary
            };
        }

        public async Task<MonthlyReportDto> GetMonthlyReportAsync(int? year = null, int? month = null)
        {
            var targetYear = year ?? DateTime.Now.Year;
            var targetMonth = month ?? DateTime.Now.Month;

            var monthStart = new DateTime(targetYear, targetMonth, 1);
            var monthEnd = monthStart.AddMonths(1);

            var summary = new MonthlySummaryDto
            {
                TotalWorkDays = (monthEnd - monthStart).Days,
                TotalCheckIns = await _qrCodeScanRepository.CountAsync(x =>
                    x.ScanDate >= monthStart && x.ScanDate < monthEnd &&
                    x.ScanType == ScanType.CheckIn),
                TotalActiveUsers = await _qrCodeScanRepository.GetAll()
                    .Where(x => x.ScanDate >= monthStart && x.ScanDate < monthEnd)
                    .Select(x => x.UserId)
                    .Distinct()
                    .CountAsync(),
                TotalActiveProjects = await _projectRepository.CountAsync(x => x.IsActive),
                TotalRevenue = await _projectRepository.GetAll()
                    .Where(x => x.StartDate >= monthStart && x.StartDate < monthEnd)
                    .SumAsync(x => x.Budget ?? 0),
                TotalExpenses = await _projectRepository.GetAll()
                    .Where(x => x.StartDate >= monthStart && x.StartDate < monthEnd)
                    .SumAsync(x => x.SpentAmount ?? 0),
                ProjectsCompleted = await _projectRepository.CountAsync(x =>
                    x.Status == ProjectStatus.Completed &&
                    x.EndDate.HasValue && x.EndDate >= monthStart && x.EndDate < monthEnd)
            };

            var weeklyBreakdown = new List<WeeklySummaryDto>();
            for (var week = monthStart; week < monthEnd; week = week.AddDays(7))
            {
                var weekEnd = week.AddDays(7) > monthEnd ? monthEnd : week.AddDays(7);
                var weeklyReport = await GetWeeklyReportAsync(week);
                weeklyBreakdown.Add(weeklyReport.Summary);
            }

            return new MonthlyReportDto
            {
                Year = targetYear,
                Month = targetMonth,
                MonthName = monthStart.ToString("MMMM", new CultureInfo("tr-TR")),
                Summary = summary,
                WeeklyBreakdown = weeklyBreakdown
            };
        }

        public async Task<CompanyPerformanceReportDto> GetCompanyPerformanceReportAsync(Guid? companyId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException("Company performance report will be implemented based on specific requirements");
        }

        public async Task<UserPerformanceReportDto> GetUserPerformanceReportAsync(long userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException("User performance report will be implemented based on specific requirements");
        }

        public async Task<ProjectProgressReportDto> GetProjectProgressReportAsync(Guid projectId)
        {
            throw new NotImplementedException("Project progress report will be implemented based on specific requirements");
        }

        public async Task<FinancialReportDto> GetFinancialReportAsync(Guid? companyId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException("Financial report will be implemented based on specific requirements");
        }

        public async Task<byte[]> ExportToExcelAsync(ExportReportRequestDto input)
        {
            throw new NotImplementedException("Excel export will be implemented using EPPlus or similar library");
        }

        public async Task<byte[]> ExportToPdfAsync(ExportReportRequestDto input)
        {
            throw new NotImplementedException("PDF export will be implemented using iTextSharp or similar library");
        }

        public async Task<List<ReportTypeDto>> GetReportTypesAsync()
        {
            return new List<ReportTypeDto>
            {
                new ReportTypeDto
                {
                    Key = "dashboard",
                    Name = "Dashboard Raporu",
                    Description = "Genel sistem istatistikleri",
                    RequiredParameters = new List<string>(),
                    OptionalParameters = new List<string> { "startDate", "endDate" }
                },
                new ReportTypeDto
                {
                    Key = "projects",
                    Name = "Proje Raporu",
                    Description = "Proje performans analizi",
                    RequiredParameters = new List<string>(),
                    OptionalParameters = new List<string> { "companyId", "startDate", "endDate" }
                },
                new ReportTypeDto
                {
                    Key = "personnel",
                    Name = "Personel Raporu",
                    Description = "Personel performans ve çalışma saatleri",
                    RequiredParameters = new List<string>(),
                    OptionalParameters = new List<string> { "companyId", "startDate", "endDate" }
                },
                new ReportTypeDto
                {
                    Key = "qr-scans",
                    Name = "QR Tarama Raporu",
                    Description = "QR kod tarama istatistikleri",
                    RequiredParameters = new List<string>(),
                    OptionalParameters = new List<string> { "projectId", "userId", "startDate", "endDate" }
                }
            };
        }

        public async Task<CustomReportResultDto> CreateCustomReportAsync(CustomReportRequestDto input)
        {
            throw new NotImplementedException("Custom report creation will be implemented based on specific requirements");
        }

        #region Private Methods

        private async Task<User> GetCurrentUserAsync()
        {
            var userId = AbpSession.GetUserId();
            return await _userRepository.GetAsync(userId);
        }

        private async Task<DashboardStatsDto> GetDashboardStatsInternalAsync(Guid? companyId)
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

        #endregion
    }
}

