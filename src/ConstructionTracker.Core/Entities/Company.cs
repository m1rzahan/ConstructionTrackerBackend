using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ConstructionTracker.Entities
{
    public class Company : FullAuditedEntity<Guid>
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(50)]
        public string TaxNumber { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Authorization.Users.User> Users { get; set; }
        public virtual ICollection<Project> Projects { get; set; }

        public Company()
        {
            Users = new HashSet<Authorization.Users.User>();
            Projects = new HashSet<Project>();
        }
    }
} 