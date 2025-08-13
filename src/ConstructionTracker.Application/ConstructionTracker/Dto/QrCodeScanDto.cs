using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using ConstructionTracker.Entities;
using System;

namespace ConstructionTracker.ConstructionTracker.Dto
{
    [AutoMapFrom(typeof(QrCodeScan))]
    public class QrCodeScanDto : EntityDto<Guid>
    {
        public string QrCodeData { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public Guid? ProjectId { get; set; }
        public string ProjectName { get; set; }
        public DateTime ScanDate { get; set; }
        public string Location { get; set; }
        public ScanType ScanType { get; set; }
        public string Notes { get; set; }
        public bool IsValid { get; set; }
        public string AdditionalData { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime CreationTime { get; set; }

        public string ScanTypeDisplayName => GetScanTypeDisplayName();

        private string GetScanTypeDisplayName()
        {
            return ScanType switch
            {
                ScanType.CheckIn => "Giriş",
                ScanType.CheckOut => "Çıkış",
                ScanType.MaterialScan => "Malzeme Tarama",
                ScanType.LocationScan => "Konum Tarama",
                ScanType.EquipmentScan => "Ekipman Tarama",
                ScanType.Other => "Diğer",
                _ => "Bilinmiyor"
            };
        }
    }

    public class CreateQrCodeScanDto
    {
        public string QrCodeData { get; set; }
        public long UserId { get; set; }
        public Guid? ProjectId { get; set; }
        public DateTime ScanDate { get; set; } = DateTime.Now;
        public string Location { get; set; }
        public ScanType ScanType { get; set; }
        public string Notes { get; set; }
        public string AdditionalData { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class QrCodeScanStatsDto
    {
        public int TotalScans { get; set; }
        public int TodayScans { get; set; }
        public int CheckInScans { get; set; }
        public int CheckOutScans { get; set; }
        public int MaterialScans { get; set; }
        public int LocationScans { get; set; }
        public int EquipmentScans { get; set; }
    }
} 