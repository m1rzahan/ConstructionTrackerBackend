using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;

namespace ConstructionTracker.Entities
{
    public class QrCodeScan : FullAuditedEntity<Guid>
    {
        [Required]
        [StringLength(500)]
        public string QrCodeData { get; set; }

        public long UserId { get; set; }
        public Guid? ProjectId { get; set; }

        public DateTime ScanDate { get; set; }

        [StringLength(200)]
        public string Location { get; set; }

        public ScanType ScanType { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        public bool IsValid { get; set; } = true;

        [StringLength(500)]
        public string AdditionalData { get; set; } // JSON format için

        // GPS koordinatları
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Navigation properties
        public virtual Authorization.Users.User User { get; set; }
        public virtual Project Project { get; set; }
    }

    public enum ScanType
    {
        CheckIn = 1,
        CheckOut = 2,
        MaterialScan = 3,
        LocationScan = 4,
        EquipmentScan = 5,
        Other = 99
    }
} 