using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using ConstructionTracker.ConstructionTracker.Dto;
using ConstructionTracker.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConstructionTracker.ConstructionTracker.Services
{
    [AbpAuthorize]
    public class QrCodeScanAppService : ConstructionTrackerAppServiceBase, IQrCodeScanAppService
    {
        private readonly IRepository<QrCodeScan, Guid> _qrCodeScanRepository;
        private readonly IRepository<ActivityLog, Guid> _activityLogRepository;

        public QrCodeScanAppService(
            IRepository<QrCodeScan, Guid> qrCodeScanRepository,
            IRepository<ActivityLog, Guid> activityLogRepository)
        {
            _qrCodeScanRepository = qrCodeScanRepository;
            _activityLogRepository = activityLogRepository;
        }

        public async Task<QrCodeScanDto> ScanQrCodeAsync(CreateQrCodeScanDto input)
        {
            var scan = ObjectMapper.Map<QrCodeScan>(input);
            scan = await _qrCodeScanRepository.InsertAsync(scan);

            // Log activity
            await LogActivityAsync(new CreateActivityLogDto
            {
                Title = "QR Kod Tarandı",
                Description = $"QR kod tarandı: {input.QrCodeData}",
                ActivityType = ActivityType.QrCodeScanned,
                UserId = input.UserId,
                ProjectId = input.ProjectId,
                ActivityDate = DateTime.Now
            });

            return ObjectMapper.Map<QrCodeScanDto>(scan);
        }

        public async Task<bool> ValidateQrCodeAsync(string qrCodeData)
        {
            // Simple validation - in real app, implement actual QR code validation logic
            return !string.IsNullOrEmpty(qrCodeData) && qrCodeData.Length >= 5;
        }

        public async Task<PagedResultDto<QrCodeScanDto>> GetUserScansAsync(long userId, PagedAndSortedResultRequestDto input)
        {
            var query = _qrCodeScanRepository.GetAll()
                .Include(x => x.User)
                .Include(x => x.Project)
                .Where(x => x.UserId == userId);

            var totalCount = await query.CountAsync();
            
            var scans = await query
                .OrderByDescending(x => x.ScanDate)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var scanDtos = ObjectMapper.Map<List<QrCodeScanDto>>(scans);
            
            return new PagedResultDto<QrCodeScanDto>(totalCount, scanDtos);
        }

        public async Task<PagedResultDto<QrCodeScanDto>> GetProjectScansAsync(Guid projectId, PagedAndSortedResultRequestDto input)
        {
            var query = _qrCodeScanRepository.GetAll()
                .Include(x => x.User)
                .Include(x => x.Project)
                .Where(x => x.ProjectId == projectId);

            var totalCount = await query.CountAsync();
            
            var scans = await query
                .OrderByDescending(x => x.ScanDate)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var scanDtos = ObjectMapper.Map<List<QrCodeScanDto>>(scans);
            
            return new PagedResultDto<QrCodeScanDto>(totalCount, scanDtos);
        }

        public async Task<List<QrCodeScanDto>> GetTodayScansAsync(long? userId = null)
        {
            var today = DateTime.Today;
            var query = _qrCodeScanRepository.GetAll()
                .Include(x => x.User)
                .Include(x => x.Project)
                .Where(x => x.ScanDate >= today && x.ScanDate < today.AddDays(1));

            if (userId.HasValue)
            {
                query = query.Where(x => x.UserId == userId.Value);
            }

            var scans = await query
                .OrderByDescending(x => x.ScanDate)
                .ToListAsync();

            return ObjectMapper.Map<List<QrCodeScanDto>>(scans);
        }

        public async Task<QrCodeScanDto> CheckInAsync(CreateQrCodeScanDto input)
        {
            input.ScanType = ScanType.CheckIn;
            return await ScanQrCodeAsync(input);
        }

        public async Task<QrCodeScanDto> CheckOutAsync(CreateQrCodeScanDto input)
        {
            input.ScanType = ScanType.CheckOut;
            return await ScanQrCodeAsync(input);
        }

        public async Task<bool> IsUserCheckedInAsync(long userId, Guid? projectId = null)
        {
            var today = DateTime.Today;
            
            var lastCheckIn = await _qrCodeScanRepository.GetAll()
                .Where(x => x.UserId == userId && 
                           x.ScanDate >= today &&
                           x.ScanType == ScanType.CheckIn &&
                           (projectId == null || x.ProjectId == projectId))
                .OrderByDescending(x => x.ScanDate)
                .FirstOrDefaultAsync();

            if (lastCheckIn == null)
                return false;

            var lastCheckOut = await _qrCodeScanRepository.GetAll()
                .Where(x => x.UserId == userId && 
                           x.ScanDate >= today &&
                           x.ScanDate > lastCheckIn.ScanDate &&
                           x.ScanType == ScanType.CheckOut &&
                           (projectId == null || x.ProjectId == projectId))
                .OrderByDescending(x => x.ScanDate)
                .FirstOrDefaultAsync();

            return lastCheckOut == null;
        }

        public async Task<QrCodeScanStatsDto> GetScanStatsAsync(long? userId = null, Guid? projectId = null)
        {
            var query = _qrCodeScanRepository.GetAll().AsQueryable();

            if (userId.HasValue)
                query = query.Where(x => x.UserId == userId.Value);

            if (projectId.HasValue)
                query = query.Where(x => x.ProjectId == projectId.Value);

            var today = DateTime.Today;

            var totalScans = await query.CountAsync();
            var todayScans = await query.Where(x => x.ScanDate >= today).CountAsync();
            var checkInScans = await query.Where(x => x.ScanType == ScanType.CheckIn).CountAsync();
            var checkOutScans = await query.Where(x => x.ScanType == ScanType.CheckOut).CountAsync();
            var materialScans = await query.Where(x => x.ScanType == ScanType.MaterialScan).CountAsync();
            var locationScans = await query.Where(x => x.ScanType == ScanType.LocationScan).CountAsync();
            var equipmentScans = await query.Where(x => x.ScanType == ScanType.EquipmentScan).CountAsync();

            return new QrCodeScanStatsDto
            {
                TotalScans = totalScans,
                TodayScans = todayScans,
                CheckInScans = checkInScans,
                CheckOutScans = checkOutScans,
                MaterialScans = materialScans,
                LocationScans = locationScans,
                EquipmentScans = equipmentScans
            };
        }

        public async Task<List<QrCodeScanDto>> GetRecentScansAsync(int count = 10)
        {
            var scans = await _qrCodeScanRepository.GetAll()
                .Include(x => x.User)
                .Include(x => x.Project)
                .OrderByDescending(x => x.ScanDate)
                .Take(count)
                .ToListAsync();

            return ObjectMapper.Map<List<QrCodeScanDto>>(scans);
        }

        public async Task<object> GenerateTestQrCodeAsync()
        {
            var testQrCodeText = "TEST_QR_CODE_" + DateTime.Now.Ticks;
            var testQrData = new
            {
                type = "test",
                projectId = Guid.NewGuid().ToString(),
                projectName = "Test Projesi",
                location = "Test Lokasyonu",
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                data = testQrCodeText
            };

            return new
            {
                qrCodeData = System.Text.Json.JsonSerializer.Serialize(testQrData),
                qrCodeText = testQrCodeText,
                projectId = testQrData.projectId,
                projectName = testQrData.projectName,
                location = testQrData.location,
                timestamp = testQrData.timestamp
            };
        }

        private async Task LogActivityAsync(CreateActivityLogDto input)
        {
            var activity = ObjectMapper.Map<ActivityLog>(input);
            await _activityLogRepository.InsertAsync(activity);
        }
    }
} 