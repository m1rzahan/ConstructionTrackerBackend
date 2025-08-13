using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;

namespace ConstructionTracker.Entities
{
    public class ProjectMaterial : FullAuditedEntity<Guid>
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public Guid ProjectId { get; set; }

        [Required]
        [StringLength(50)]
        public string Unit { get; set; } // kg, m2, m3, adet, etc.

        public decimal RequiredQuantity { get; set; }
        public decimal UsedQuantity { get; set; } = 0;
        public decimal RemainingQuantity => RequiredQuantity - UsedQuantity;

        public decimal UnitPrice { get; set; }
        public decimal TotalCost => RequiredQuantity * UnitPrice;

        [StringLength(100)]
        public string Supplier { get; set; }

        [StringLength(100)]
        public string Brand { get; set; }

        [StringLength(100)]
        public string Model { get; set; }

        public DateTime? OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }

        public MaterialStatus Status { get; set; } = MaterialStatus.Planned;

        [StringLength(500)]
        public string Notes { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Project Project { get; set; }
    }

    public enum MaterialStatus
    {
        Planned = 1,
        Ordered = 2,
        InTransit = 3,
        Delivered = 4,
        InUse = 5,
        Consumed = 6,
        Cancelled = 7
    }
} 