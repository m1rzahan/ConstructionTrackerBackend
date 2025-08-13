using Abp.Application.Services;
using ConstructionTracker.ConstructionTracker.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConstructionTracker.ConstructionTracker.Services
{
    public interface IReportsAppService : IApplicationService
    {
        // Main reports
        Task<DashboardReportDto> GetDashboardReportAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<ProjectReportDto> GetProjectsReportAsync(Guid? companyId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<PersonnelReportDto> GetPersonnelReportAsync(Guid? companyId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<QrScanReportDto> GetQrScanReportAsync(Guid? projectId = null, long? userId = null, DateTime? startDate = null, DateTime? endDate = null);

        // Time-based reports
        Task<DailyActivityReportDto> GetDailyActivityReportAsync(DateTime date);
        Task<WeeklyReportDto> GetWeeklyReportAsync(DateTime? weekStart = null);
        Task<MonthlyReportDto> GetMonthlyReportAsync(int? year = null, int? month = null);

        // Performance reports
        Task<CompanyPerformanceReportDto> GetCompanyPerformanceReportAsync(Guid? companyId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<UserPerformanceReportDto> GetUserPerformanceReportAsync(long userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ProjectProgressReportDto> GetProjectProgressReportAsync(Guid projectId);
        Task<FinancialReportDto> GetFinancialReportAsync(Guid? companyId = null, DateTime? startDate = null, DateTime? endDate = null);

        // Export functionality
        Task<byte[]> ExportToExcelAsync(ExportReportRequestDto input);
        Task<byte[]> ExportToPdfAsync(ExportReportRequestDto input);

        // Utility methods
        Task<List<ReportTypeDto>> GetReportTypesAsync();
        Task<CustomReportResultDto> CreateCustomReportAsync(CustomReportRequestDto input);
    }
}
