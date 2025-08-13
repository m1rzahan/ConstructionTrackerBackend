using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;

namespace ConstructionTracker.ConstructionTracker.Dto
{
    // Personnel DTOs
    public class PagedPersonnelRequestDto : PagedAndSortedResultRequestDto
    {
        public string SearchTerm { get; set; }
        public int? Role { get; set; }
        public Guid? CompanyId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class CreatePersonnelDto
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public int Role { get; set; }
        public Guid? CompanyId { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Position { get; set; }
        public decimal? Salary { get; set; }
        public DateTime? HireDate { get; set; }
    }

    public class UpdatePersonnelDto
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Role { get; set; }
        public Guid? CompanyId { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Position { get; set; }
        public decimal? Salary { get; set; }
        public bool IsActive { get; set; }
    }

    public class PersonnelStatsDto
    {
        public long PersonnelId { get; set; }
        public string PersonnelName { get; set; }
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int TotalScans { get; set; }
        public int TotalWorkDays { get; set; }
        public DateTime? LastActivity { get; set; }
        public decimal TotalWorkingHours { get; set; }
        public string Role { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    public class GeneralPersonnelStatsDto
    {
        public int TotalPersonnel { get; set; }
        public int ActivePersonnel { get; set; }
        public int InactivePersonnel { get; set; }
        public int TodayCheckins { get; set; }
        public int AdminCount { get; set; }
        public int OfficeStaffCount { get; set; }
        public int SiteStaffCount { get; set; }
        public int SubcontractorCount { get; set; }
        public List<PersonnelByCompanyDto> PersonnelByCompany { get; set; }
    }

    public class PersonnelByCompanyDto
    {
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int PersonnelCount { get; set; }
    }
}

