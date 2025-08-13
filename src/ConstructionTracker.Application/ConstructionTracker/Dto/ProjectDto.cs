using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using ConstructionTracker.Entities;
using System;
using System.Collections.Generic;

namespace ConstructionTracker.ConstructionTracker.Dto
{
    [AutoMapFrom(typeof(Project))]
    public class ProjectDto : EntityDto<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public ProjectStatus Status { get; set; }
        public decimal Progress { get; set; }
        public decimal? Budget { get; set; }
        public decimal? SpentAmount { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreationTime { get; set; }
        public List<ConstructionTrackerUserDto> AssignedUsers { get; set; }
    }

    public class CreateProjectDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid CompanyId { get; set; }
        public string Address { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public decimal? Budget { get; set; }
        public string Notes { get; set; }
        public List<long> AssignedUserIds { get; set; } = new List<long>();
    }

    public class UpdateProjectDto : EntityDto<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public ProjectStatus Status { get; set; }
        public decimal Progress { get; set; }
        public decimal? Budget { get; set; }
        public decimal? SpentAmount { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
    }

    public class ProjectSummaryDto : EntityDto<Guid>
    {
        public string Name { get; set; }
        public ProjectStatus Status { get; set; }
        public decimal Progress { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public string StatusDisplayName => GetStatusDisplayName();
        
        private string GetStatusDisplayName()
        {
            return Status switch
            {
                ProjectStatus.Planning => "Planlama",
                ProjectStatus.Active => "Aktif",
                ProjectStatus.OnHold => "Beklemede",
                ProjectStatus.Completed => "Tamamlandı",
                ProjectStatus.Cancelled => "İptal",
                ProjectStatus.Delayed => "Gecikme",
                _ => "Bilinmiyor"
            };
        }
    }

    public class ProjectStatsDto
    {
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int CompletedProjects { get; set; }
        public int DelayedProjects { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AverageProgress { get; set; }
    }
} 