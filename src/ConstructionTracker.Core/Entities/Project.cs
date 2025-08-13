using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ConstructionTracker.Entities
{
    public class Project : FullAuditedEntity<Guid>
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public Guid CompanyId { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? PlannedEndDate { get; set; }

        public ProjectStatus Status { get; set; } = ProjectStatus.Active;

        public decimal Progress { get; set; } = 0; // 0-100 arası

        public decimal? Budget { get; set; }

        public decimal? SpentAmount { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Company Company { get; set; }
        public virtual ICollection<UserProject> UserProjects { get; set; }
        public virtual ICollection<ActivityLog> ActivityLogs { get; set; }
        public virtual ICollection<QrCodeScan> QrCodeScans { get; set; }
        public virtual ICollection<ProjectMaterial> ProjectMaterials { get; set; }
        public virtual ICollection<ProjectTask> ProjectTasks { get; set; }

        public Project()
        {
            UserProjects = new HashSet<UserProject>();
            ActivityLogs = new HashSet<ActivityLog>();
            QrCodeScans = new HashSet<QrCodeScan>();
            ProjectMaterials = new HashSet<ProjectMaterial>();
            ProjectTasks = new HashSet<ProjectTask>();
        }
    }

    public enum ProjectStatus
    {
        Planning = 1,
        Active = 2,
        OnHold = 3,
        Completed = 4,
        Cancelled = 5,
        Delayed = 6
    }
} 