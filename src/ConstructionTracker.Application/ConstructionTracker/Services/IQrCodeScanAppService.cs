using Abp.Application.Services;
using Abp.Application.Services.Dto;
using ConstructionTracker.ConstructionTracker.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConstructionTracker.ConstructionTracker.Services
{
    public interface IQrCodeScanAppService : IApplicationService
    {
        // QR Code Scanning
        Task<QrCodeScanDto> ScanQrCodeAsync(CreateQrCodeScanDto input);
        Task<bool> ValidateQrCodeAsync(string qrCodeData);

        // Scan History
        Task<PagedResultDto<QrCodeScanDto>> GetUserScansAsync(long userId, PagedAndSortedResultRequestDto input);
        Task<PagedResultDto<QrCodeScanDto>> GetProjectScansAsync(Guid projectId, PagedAndSortedResultRequestDto input);
        Task<List<QrCodeScanDto>> GetTodayScansAsync(long? userId = null);

        // Check-in/Check-out
        Task<QrCodeScanDto> CheckInAsync(CreateQrCodeScanDto input);
        Task<QrCodeScanDto> CheckOutAsync(CreateQrCodeScanDto input);
        Task<bool> IsUserCheckedInAsync(long userId, Guid? projectId = null);

        // Statistics
        Task<QrCodeScanStatsDto> GetScanStatsAsync(long? userId = null, Guid? projectId = null);
        Task<List<QrCodeScanDto>> GetRecentScansAsync(int count = 10);

        // Test
        Task<object> GenerateTestQrCodeAsync();
    }
} 