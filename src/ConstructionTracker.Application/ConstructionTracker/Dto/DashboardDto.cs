using System;
using System.Collections.Generic;

namespace ConstructionTracker.ConstructionTracker.Dto
{
    public class DashboardDto
    {
        public DashboardStatsDto Stats { get; set; }
        public List<ProjectSummaryDto> RecentProjects { get; set; }
        public List<DashboardActivityDto> RecentActivities { get; set; }
        public ConstructionTrackerUserDto CurrentUser { get; set; }
    }

    public class DashboardStatsDto
    {
        public int ActiveProjects { get; set; }
        public int TotalPersonnel { get; set; }
        public int MonthlyCheckins { get; set; }
        public int PendingTasks { get; set; }
        public string ActiveProjectsTrend { get; set; }
        public string TotalPersonnelTrend { get; set; }
        public string MonthlyCheckinsTrend { get; set; }
        public string PendingTasksTrend { get; set; }
    }

    public class DashboardProjectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Progress { get; set; }
        public string Status { get; set; }
        public string Deadline { get; set; }
        public string StatusColor { get; set; }
    }

    public class DashboardUserStatsDto
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public int ProjectCount { get; set; }
        public int CheckinCount { get; set; }
        public DateTime? LastActivity { get; set; }
        public string UserRole { get; set; }
    }

    public class WeeklyStatsDto
    {
        public List<DailyStatDto> DailyStats { get; set; }
        public int TotalCheckins { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveProjects { get; set; }
    }

    public class DailyStatDto
    {
        public DateTime Date { get; set; }
        public int CheckinCount { get; set; }
        public int ActiveUsers { get; set; }
        public string DayName { get; set; }
    }
} 