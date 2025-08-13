using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;

namespace ConstructionTracker.Entities
{
    public class UserProject : FullAuditedEntity<Guid>
    {
        public long UserId { get; set; }
        public Guid ProjectId { get; set; }

        [StringLength(100)]
        public string Role { get; set; } // Projede sahip olduğu rol

        public DateTime AssignedDate { get; set; }
        public DateTime? UnassignedDate { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string Notes { get; set; }

        // Navigation properties
        public virtual Authorization.Users.User User { get; set; }
        public virtual Project Project { get; set; }
    }
} 