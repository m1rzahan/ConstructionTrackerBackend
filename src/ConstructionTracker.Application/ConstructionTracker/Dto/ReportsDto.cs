using System;
using System.Collections.Generic;

namespace ConstructionTracker.ConstructionTracker.Dto
{
    // Report DTOs
    public class DashboardReportDto
    {
        public DateTime ReportDate { get; set; }
        public DashboardStatsDto Stats { get; set; }
        public List<ProjectSummaryDto> TopProjects { get; set; }
        public List<ConstructionTrackerUserDto> TopPersonnel { get; set; }
        public List<DailyActivityDto> RecentActivities { get; set; }
    }

    public class ProjectReportDto
    {
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int CompletedProjects { get; set; }
        public int DelayedProjects { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AverageProgress { get; set; }
        public List<ProjectSummaryDto> Projects { get; set; }
        public List<ProjectBudgetDto> BudgetBreakdown { get; set; }
    }

    public class PersonnelReportDto
    {
        public int TotalPersonnel { get; set; }
        public int ActivePersonnel { get; set; }
        public int TotalCheckIns { get; set; }
        public decimal AverageWorkingHours { get; set; }
        public List<PersonnelPerformanceDto> PersonnelPerformance { get; set; }
        public List<DepartmentStatsDto> DepartmentStats { get; set; }
    }

    public class QrScanReportDto
    {
        public int TotalScans { get; set; }
        public int TotalCheckIns { get; set; }
        public int TotalCheckOuts { get; set; }
        public int UniqueUsers { get; set; }
        public List<QrCodeScanDto> RecentScans { get; set; }
        public List<HourlyActivityDto> HourlyActivity { get; set; }
    }

    public class DailyActivityReportDto
    {
        public DateTime Date { get; set; }
        public int TotalCheckIns { get; set; }
        public int TotalCheckOuts { get; set; }
        public int ActiveUsers { get; set; }
        public int ActiveProjects { get; set; }
        public List<QrCodeScanDto> Activities { get; set; }
    }

    public class WeeklyReportDto
    {
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public List<DailyStatDto> DailyStats { get; set; }
        public WeeklySummaryDto Summary { get; set; }
    }

    public class MonthlyReportDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public MonthlySummaryDto Summary { get; set; }
        public List<WeeklySummaryDto> WeeklyBreakdown { get; set; }
    }

    public class CompanyPerformanceReportDto
    {
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int TotalProjects { get; set; }
        public int TotalPersonnel { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal Profitability { get; set; }
        public List<ProjectPerformanceDto> ProjectPerformances { get; set; }
    }

    public class UserPerformanceReportDto
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public int TotalWorkDays { get; set; }
        public decimal TotalWorkingHours { get; set; }
        public int TotalProjects { get; set; }
        public decimal AverageRating { get; set; }
        public List<UserDailyPerformanceDto> DailyPerformance { get; set; }
    }

    public class ProjectProgressReportDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; }
        public decimal CurrentProgress { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public List<ProjectMilestoneDto> Milestones { get; set; }
        public List<ProjectProgressHistoryDto> ProgressHistory { get; set; }
    }

    public class FinancialReportDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProjectBudgets { get; set; }
        public decimal PersonnelCosts { get; set; }
        public List<FinancialBreakdownDto> MonthlyBreakdown { get; set; }
    }

    public class ExportReportRequestDto
    {
        public string ReportType { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CustomReportRequestDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Fields { get; set; }
        public Dictionary<string, object> Filters { get; set; }
        public string GroupBy { get; set; }
        public string OrderBy { get; set; }
    }

    public class CustomReportResultDto
    {
        public string ReportName { get; set; }
        public DateTime GeneratedAt { get; set; }
        public List<Dictionary<string, object>> Data { get; set; }
        public int TotalRecords { get; set; }
    }

    public class ReportTypeDto
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> RequiredParameters { get; set; }
        public List<string> OptionalParameters { get; set; }
    }

    // Additional helper DTOs
    public class ProjectBudgetDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; }
        public decimal Budget { get; set; }
        public decimal Spent { get; set; }
        public decimal Remaining { get; set; }
        public decimal PercentageUsed { get; set; }
    }

    public class PersonnelPerformanceDto
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public int WorkDays { get; set; }
        public decimal WorkingHours { get; set; }
        public int ProjectCount { get; set; }
        public decimal PerformanceScore { get; set; }
    }

    public class DepartmentStatsDto
    {
        public string Department { get; set; }
        public int PersonnelCount { get; set; }
        public decimal TotalWorkingHours { get; set; }
        public decimal AveragePerformance { get; set; }
    }

    public class HourlyActivityDto
    {
        public int Hour { get; set; }
        public int CheckInCount { get; set; }
        public int CheckOutCount { get; set; }
    }

    public class WeeklySummaryDto
    {
        public int TotalWorkDays { get; set; }
        public int TotalCheckIns { get; set; }
        public int TotalActiveUsers { get; set; }
        public int TotalActiveProjects { get; set; }
        public decimal AverageWorkingHours { get; set; }
    }

    public class MonthlySummaryDto
    {
        public int TotalWorkDays { get; set; }
        public int TotalCheckIns { get; set; }
        public int TotalActiveUsers { get; set; }
        public int TotalActiveProjects { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal ProjectsCompleted { get; set; }
    }

    public class ProjectPerformanceDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; }
        public decimal Progress { get; set; }
        public decimal BudgetUtilization { get; set; }
        public int DaysOverdue { get; set; }
        public decimal PerformanceScore { get; set; }
    }

    public class UserDailyPerformanceDto
    {
        public DateTime Date { get; set; }
        public decimal WorkingHours { get; set; }
        public int CheckInTime { get; set; }
        public int CheckOutTime { get; set; }
        public string Activities { get; set; }
    }

    public class ProjectMilestoneDto
    {
        public string Name { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsCompleted { get; set; }
        public string Status { get; set; }
    }

    public class ProjectProgressHistoryDto
    {
        public DateTime Date { get; set; }
        public decimal Progress { get; set; }
        public string Notes { get; set; }
    }

    public class FinancialBreakdownDto
    {
        public string Period { get; set; }
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
        public decimal Profit { get; set; }
        public decimal Margin { get; set; }
    }

    public class DailyActivityDto
    {
        public DateTime Date { get; set; }
        public string Activity { get; set; }
        public string User { get; set; }
        public string Project { get; set; }
        public string Type { get; set; }
    }
}

