using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;

namespace ConstructionTracker.Entities
{
    public class ActivityLog : FullAuditedEntity<Guid>
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public ActivityType ActivityType { get; set; }

        public long? UserId { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? CompanyId { get; set; }

        public DateTime ActivityDate { get; set; }

        [StringLength(200)]
        public string Location { get; set; }

        [StringLength(500)]
        public string AdditionalData { get; set; } // JSON format için

        public ActivityPriority Priority { get; set; } = ActivityPriority.Normal;

        public bool IsRead { get; set; } = false;

        // Navigation properties
        public virtual Authorization.Users.User User { get; set; }
        public virtual Project Project { get; set; }
        public virtual Company Company { get; set; }
        
        // Computed properties for reports
        public string UserName => User != null ? $"{User.FirstName} {User.LastName}" : null;
        public string ProjectName => Project?.Name;
    }

    public enum ActivityType
    {
        UserLogin = 1,
        UserLogout = 2,
        ProjectCreated = 3,
        ProjectUpdated = 4,
        ProjectCompleted = 5,
        QrCodeScanned = 6,
        ReportGenerated = 7,
        MaterialAdded = 8,
        MaterialUsed = 9,
        PersonnelAssigned = 10,
        PersonnelRemoved = 11,
        Other = 99
    }

    public enum ActivityPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Critical = 4
    }
} 