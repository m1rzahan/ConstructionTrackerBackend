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
    public class QrCodeController : AbpController
    {
        private readonly IQrCodeScanAppService _qrCodeScanAppService;

        public QrCodeController(IQrCodeScanAppService qrCodeScanAppService)
        {
            _qrCodeScanAppService = qrCodeScanAppService;
        }

        /// <summary>
        /// QR kod tara (Android uygulaması için ana endpoint)
        /// </summary>
        [HttpPost("scan")]
        public async Task<QrCodeScanDto> ScanQrCode([FromBody] CreateQrCodeScanDto input)
        {
            return await _qrCodeScanAppService.ScanQrCodeAsync(input);
        }

        /// <summary>
        /// QR kodunu doğrula
        /// </summary>
        [HttpPost("validate")]
        public async Task<bool> ValidateQrCode([FromBody] ValidateQrCodeRequestDto input)
        {
            return await _qrCodeScanAppService.ValidateQrCodeAsync(input.QrCodeData);
        }

        /// <summary>
        /// Kullanıcının QR kod tarama geçmişini getir
        /// </summary>
        [HttpGet("user/{userId}/scans")]
        public async Task<PagedResultDto<QrCodeScanDto>> GetUserScans(
            long userId,
            [FromQuery] PagedAndSortedResultRequestDto input)
        {
            return await _qrCodeScanAppService.GetUserScansAsync(userId, input);
        }

        /// <summary>
        /// Projeye ait QR kod taramalarını getir
        /// </summary>
        [HttpGet("project/{projectId}/scans")]
        public async Task<PagedResultDto<QrCodeScanDto>> GetProjectScans(
            Guid projectId,
            [FromQuery] PagedAndSortedResultRequestDto input)
        {
            return await _qrCodeScanAppService.GetProjectScansAsync(projectId, input);
        }

        /// <summary>
        /// Bugünkü taramaları getir
        /// </summary>
        [HttpGet("today")]
        public async Task<List<QrCodeScanDto>> GetTodayScans([FromQuery] long? userId = null)
        {
            return await _qrCodeScanAppService.GetTodayScansAsync(userId);
        }

        /// <summary>
        /// Check-in işlemi (giriş)
        /// </summary>
        [HttpPost("checkin")]
        public async Task<QrCodeScanDto> CheckIn([FromBody] CreateQrCodeScanDto input)
        {
            return await _qrCodeScanAppService.CheckInAsync(input);
        }

        /// <summary>
        /// Check-out işlemi (çıkış)
        /// </summary>
        [HttpPost("checkout")]
        public async Task<QrCodeScanDto> CheckOut([FromBody] CreateQrCodeScanDto input)
        {
            return await _qrCodeScanAppService.CheckOutAsync(input);
        }

        /// <summary>
        /// Kullanıcının giriş durumunu kontrol et
        /// </summary>
        [HttpGet("user/{userId}/checked-in")]
        public async Task<bool> IsUserCheckedIn(long userId, [FromQuery] Guid? projectId = null)
        {
            return await _qrCodeScanAppService.IsUserCheckedInAsync(userId, projectId);
        }

        /// <summary>
        /// QR kod tarama istatistiklerini getir
        /// </summary>
        [HttpGet("stats")]
        public async Task<QrCodeScanStatsDto> GetScanStats(
            [FromQuery] long? userId = null,
            [FromQuery] Guid? projectId = null)
        {
            return await _qrCodeScanAppService.GetScanStatsAsync(userId, projectId);
        }

        /// <summary>
        /// Son taramaları getir (dashboard için)
        /// </summary>
        [HttpGet("recent")]
        public async Task<List<QrCodeScanDto>> GetRecentScans([FromQuery] int count = 10)
        {
            return await _qrCodeScanAppService.GetRecentScansAsync(count);
        }

        /// <summary>
        /// Test QR kodu oluştur
        /// </summary>
        [HttpGet("generate-test")]
        public async Task<object> GenerateTestQrCode()
        {
            return await _qrCodeScanAppService.GenerateTestQrCodeAsync();
        }
    }

    // Helper DTO for validation
    public class ValidateQrCodeRequestDto
    {
        public string QrCodeData { get; set; }
    }
} 