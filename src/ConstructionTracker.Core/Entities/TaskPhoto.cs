using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using ConstructionTracker.Authorization.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConstructionTracker.Entities
{
    [Table("TaskPhotos")]
    public class TaskPhoto : FullAuditedEntity<Guid>
    {
        [Required]
        public Guid TaskId { get; set; }

        [ForeignKey("TaskId")]
        public virtual ProjectTask Task { get; set; }

        [Required]
        public long UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        [StringLength(1000)]
        public string FilePath { get; set; }

        [StringLength(500)]
        public string FileName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public long? FileSize { get; set; }

        [StringLength(100)]
        public string ContentType { get; set; }

        // Computed properties
        public string UserName => User != null ? $"{User.FirstName} {User.LastName}" : null;
    }
}
