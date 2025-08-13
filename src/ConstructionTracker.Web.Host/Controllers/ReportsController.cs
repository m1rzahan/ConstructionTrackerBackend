using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Controllers;
using ConstructionTracker.ConstructionTracker.Dto;
using ConstructionTracker.ConstructionTracker.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConstructionTracker.Web.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : AbpController
    {
        private readonly IReportsAppService _reportsAppService;

        public ReportsController(IReportsAppService reportsAppService)
        {
            _reportsAppService = reportsAppService;
        }

        /// <summary>
        /// Genel dashboard raporu
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<DashboardReportDto> GetDashboardReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            return await _reportsAppService.GetDashboardReportAsync(startDate, endDate);
        }

        /// <summary>
        /// Proje raporları
        /// </summary>
        [HttpGet("projects")]
        public async Task<ProjectReportDto> GetProjectsReport([FromQuery] Guid? companyId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            return await _reportsAppService.GetProjectsReportAsync(companyId, startDate, endDate);
        }

        /// <summary>
        /// Personel raporları
        /// </summary>
        [HttpGet("personnel")]
        public async Task<PersonnelReportDto> GetPersonnelReport([FromQuery] Guid? companyId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            return await _reportsAppService.GetPersonnelReportAsync(companyId, startDate, endDate);
        }

        /// <summary>
        /// QR kod tarama raporları
        /// </summary>
        [HttpGet("qr-scans")]
        public async Task<QrScanReportDto> GetQrScanReport([FromQuery] Guid? projectId = null, [FromQuery] long? userId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            return await _reportsAppService.GetQrScanReportAsync(projectId, userId, startDate, endDate);
        }

        /// <summary>
        /// Günlük aktivite raporu
        /// </summary>
        [HttpGet("daily-activity")]
        public async Task<DailyActivityReportDto> GetDailyActivityReport([FromQuery] DateTime date)
        {
            return await _reportsAppService.GetDailyActivityReportAsync(date);
        }

        /// <summary>
        /// Haftalık rapor
        /// </summary>
        [HttpGet("weekly")]
        public async Task<WeeklyReportDto> GetWeeklyReport([FromQuery] DateTime? weekStart = null)
        {
            return await _reportsAppService.GetWeeklyReportAsync(weekStart);
        }

        /// <summary>
        /// Aylık rapor
        /// </summary>
        [HttpGet("monthly")]
        public async Task<MonthlyReportDto> GetMonthlyReport([FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            return await _reportsAppService.GetMonthlyReportAsync(year, month);
        }

        /// <summary>
        /// Şirket performans raporu
        /// </summary>
        [HttpGet("company-performance")]
        public async Task<CompanyPerformanceReportDto> GetCompanyPerformanceReport([FromQuery] Guid? companyId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            return await _reportsAppService.GetCompanyPerformanceReportAsync(companyId, startDate, endDate);
        }

        /// <summary>
        /// Kullanıcı performans raporu
        /// </summary>
        [HttpGet("user-performance/{userId}")]
        public async Task<UserPerformanceReportDto> GetUserPerformanceReport(long userId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            return await _reportsAppService.GetUserPerformanceReportAsync(userId, startDate, endDate);
        }

        /// <summary>
        /// Proje ilerleme raporu
        /// </summary>
        [HttpGet("project-progress/{projectId}")]
        public async Task<ProjectProgressReportDto> GetProjectProgressReport(Guid projectId)
        {
            return await _reportsAppService.GetProjectProgressReportAsync(projectId);
        }

        /// <summary>
        /// Finansal rapor
        /// </summary>
        [HttpGet("financial")]
        public async Task<FinancialReportDto> GetFinancialReport([FromQuery] Guid? companyId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            return await _reportsAppService.GetFinancialReportAsync(companyId, startDate, endDate);
        }

        /// <summary>
        /// Excel raporu oluştur ve indir
        /// </summary>
        [HttpPost("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromBody] ExportReportRequestDto input)
        {
            var excelData = await _reportsAppService.ExportToExcelAsync(input);
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        /// <summary>
        /// PDF raporu oluştur ve indir
        /// </summary>
        [HttpPost("export/pdf")]
        public async Task<IActionResult> ExportToPdf([FromBody] ExportReportRequestDto input)
        {
            var pdfData = await _reportsAppService.ExportToPdfAsync(input);
            return File(pdfData, "application/pdf", $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }

        /// <summary>
        /// Rapor türlerini listele
        /// </summary>
        [HttpGet("types")]
        public async Task<List<ReportTypeDto>> GetReportTypes()
        {
            return await _reportsAppService.GetReportTypesAsync();
        }

        /// <summary>
        /// Özel rapor oluştur
        /// </summary>
        [HttpPost("custom")]
        public async Task<CustomReportResultDto> CreateCustomReport([FromBody] CustomReportRequestDto input)
        {
            return await _reportsAppService.CreateCustomReportAsync(input);
        }
    }


}
