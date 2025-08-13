using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using ConstructionTracker.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConstructionTracker.Entities
{
    [Table("ProjectTasks")]
    public class ProjectTask : FullAuditedEntity<Guid>
    {
        [Required]
        [StringLength(500)]
        public string Title { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        [Required]
        public TaskStatus Status { get; set; }

        [Required]
        public TaskPriority Priority { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        [Required]
        public long AssignedToUserId { get; set; }

        [ForeignKey("AssignedToUserId")]
        public virtual User AssignedToUser { get; set; }

        [Required]
        public long AssignedByUserId { get; set; }

        [ForeignKey("AssignedByUserId")]
        public virtual User AssignedByUser { get; set; }

        public DateTime? DueDate { get; set; }

        [StringLength(500)]
        public string Location { get; set; }

        public DateTime? CompletedDate { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        // Navigation properties
        public virtual ICollection<TaskComment> Comments { get; set; }
        public virtual ICollection<TaskPhoto> Photos { get; set; }

        public ProjectTask()
        {
            Comments = new HashSet<TaskComment>();
            Photos = new HashSet<TaskPhoto>();
        }

        // Computed properties
        public string AssignedToUserName => AssignedToUser != null ? $"{AssignedToUser.FirstName} {AssignedToUser.LastName}" : null;
        public string AssignedByUserName => AssignedByUser != null ? $"{AssignedByUser.FirstName} {AssignedByUser.LastName}" : null;
        public string ProjectName => Project?.Name;
        public int PhotoCount => Photos?.Count ?? 0;
        public int CommentCount => Comments?.Count ?? 0;
    }

    public enum TaskStatus
    {
        Todo = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4
    }

    public enum TaskPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
}
