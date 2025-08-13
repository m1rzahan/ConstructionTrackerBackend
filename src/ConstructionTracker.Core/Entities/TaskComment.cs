using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using ConstructionTracker.Authorization.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConstructionTracker.Entities
{
    [Table("TaskComments")]
    public class TaskComment : FullAuditedEntity<Guid>
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
        [StringLength(2000)]
        public string Comment { get; set; }

        // Computed properties
        public string UserName => User != null ? $"{User.FirstName} {User.LastName}" : null;
    }
}
