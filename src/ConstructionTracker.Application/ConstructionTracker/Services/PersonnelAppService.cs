using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.ObjectMapping;
using Abp.Runtime.Session;
using ConstructionTracker.Authorization.Users;
using ConstructionTracker.ConstructionTracker.Dto;
using ConstructionTracker.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConstructionTracker.ConstructionTracker.Services
{
    [AbpAuthorize]
    public class PersonnelAppService : ConstructionTrackerAppServiceBase, IPersonnelAppService
    {
        private readonly UserManager _userManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Company, Guid> _companyRepository;
        private readonly IRepository<Project, Guid> _projectRepository;
        private readonly IRepository<UserProject, Guid> _userProjectRepository;
        private readonly IRepository<QrCodeScan, Guid> _qrCodeScanRepository;

        public PersonnelAppService(
            UserManager userManager,
            IRepository<User, long> userRepository,
            IRepository<Company, Guid> companyRepository,
            IRepository<Project, Guid> projectRepository,
            IRepository<UserProject, Guid> userProjectRepository,
            IRepository<QrCodeScan, Guid> qrCodeScanRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _projectRepository = projectRepository;
            _userProjectRepository = userProjectRepository;
            _qrCodeScanRepository = qrCodeScanRepository;
        }

        public async Task<PagedResultDto<ConstructionTrackerUserDto>> GetAllPersonnelAsync(PagedPersonnelRequestDto input)
        {
            var currentUser = await GetCurrentUserAsync();
            
            var query = _userRepository.GetAll()
                .Include(x => x.Company)
                .Where(x => x.Id != currentUser.Id); // Exclude current user

            // Apply company filter for non-admin users
            if (currentUser.Role != UserRoleType.Admin && currentUser.CompanyId.HasValue)
            {
                query = query.Where(x => x.CompanyId == currentUser.CompanyId);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(input.SearchTerm))
            {
                var searchTerm = input.SearchTerm.ToLower();
                query = query.Where(x => 
                    x.FirstName.ToLower().Contains(searchTerm) ||
                    x.LastName.ToLower().Contains(searchTerm) ||
                    x.EmailAddress.ToLower().Contains(searchTerm));
            }

            // Apply role filter
            if (input.Role.HasValue)
            {
                query = query.Where(x => (int)x.Role == input.Role.Value);
            }

            // Apply company filter
            if (input.CompanyId.HasValue)
            {
                query = query.Where(x => x.CompanyId == input.CompanyId);
            }

            // Apply active filter
            if (input.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == input.IsActive.Value);
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(input.Sorting))
            {
                // Simple sorting implementation
                if (input.Sorting.ToLower().Contains("firstname"))
                {
                    query = input.Sorting.ToLower().Contains("desc") 
                        ? query.OrderByDescending(x => x.FirstName)
                        : query.OrderBy(x => x.FirstName);
                }
                else if (input.Sorting.ToLower().Contains("lastname"))
                {
                    query = input.Sorting.ToLower().Contains("desc")
                        ? query.OrderByDescending(x => x.LastName)
                        : query.OrderBy(x => x.LastName);
                }
                else
                {
                    query = query.OrderBy(x => x.FirstName);
                }
            }
            else
            {
                query = query.OrderBy(x => x.FirstName);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var dtoItems = new List<ConstructionTrackerUserDto>();
            foreach (var item in items)
            {
                var dto = ObjectMapper.Map<ConstructionTrackerUserDto>(item);
                dto.CompanyName = item.Company?.Name;
                dtoItems.Add(dto);
            }

            return new PagedResultDto<ConstructionTrackerUserDto>(totalCount, dtoItems);
        }

        public async Task<ConstructionTrackerUserDto> GetPersonnelAsync(long id)
        {
            var user = await _userRepository
                .GetAllIncluding(x => x.Company)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                throw new Abp.UI.UserFriendlyException("Personel bulunamadı");

            var dto = ObjectMapper.Map<ConstructionTrackerUserDto>(user);
            dto.CompanyName = user.Company?.Name;

            return dto;
        }

        public async Task<ConstructionTrackerUserDto> CreatePersonnelAsync(CreatePersonnelDto input)
        {
            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(input.Email);
            if (existingUser != null)
                throw new Abp.UI.UserFriendlyException("Bu e-posta adresi zaten kullanılmaktadır");

            var user = new User
            {
                EmailAddress = input.Email,
                UserName = input.Email,
                FirstName = input.FirstName,
                LastName = input.LastName,
                Role = (UserRoleType)input.Role,
                CompanyId = input.CompanyId,
                IsActive = true,
                PhoneNumber = input.PhoneNumber,
                Address = input.Address,
                BirthDate = input.BirthDate,
                Position = input.Position,
                Salary = input.Salary,
                HireDate = input.HireDate ?? DateTime.Now,
                Name = input.FirstName,
                Surname = input.LastName
            };

            var result = await _userManager.CreateAsync(user, input.Password);
            if (!result.Succeeded)
            {
                throw new Abp.UI.UserFriendlyException("Personel oluşturulurken hata oluştu: " + 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return await GetPersonnelAsync(user.Id);
        }

        public async Task<ConstructionTrackerUserDto> UpdatePersonnelAsync(UpdatePersonnelDto input)
        {
            var user = await _userRepository.GetAsync(input.Id);

            user.EmailAddress = input.Email;
            user.UserName = input.Email;
            user.FirstName = input.FirstName;
            user.LastName = input.LastName;
            user.Role = (UserRoleType)input.Role;
            user.CompanyId = input.CompanyId;
            user.IsActive = input.IsActive;
            user.PhoneNumber = input.PhoneNumber;
            user.Address = input.Address;
            user.BirthDate = input.BirthDate;
            user.Position = input.Position;
            user.Salary = input.Salary;

            await _userRepository.UpdateAsync(user);

            return await GetPersonnelAsync(user.Id);
        }

        public async Task DeletePersonnelAsync(long id)
        {
            var user = await _userRepository.GetAsync(id);
            user.IsActive = false; // Soft delete
            await _userRepository.UpdateAsync(user);
        }

        public async Task<List<ProjectSummaryDto>> GetPersonnelProjectsAsync(long personnelId)
        {
            var userProjects = await _userProjectRepository.GetAll()
                .Include(x => x.Project)
                .Where(x => x.UserId == personnelId && x.IsActive)
                .Select(x => x.Project)
                .Where(x => x.IsActive)
                .ToListAsync();

            return ObjectMapper.Map<List<ProjectSummaryDto>>(userProjects);
        }

        public async Task<PagedResultDto<QrCodeScanDto>> GetPersonnelScansAsync(long personnelId, PagedAndSortedResultRequestDto input)
        {
            var query = _qrCodeScanRepository.GetAll()
                .Include(x => x.Project)
                .Where(x => x.UserId == personnelId)
                .OrderByDescending(x => x.ScanDate);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var dtoItems = ObjectMapper.Map<List<QrCodeScanDto>>(items);

            return new PagedResultDto<QrCodeScanDto>(totalCount, dtoItems);
        }

        public async Task<PersonnelStatsDto> GetPersonnelStatsAsync(long personnelId)
        {
            var user = await _userRepository.GetAsync(personnelId);

            var totalProjects = await _userProjectRepository.CountAsync(x => x.UserId == personnelId);
            var activeProjects = await _userProjectRepository.CountAsync(x => 
                x.UserId == personnelId && x.IsActive && x.Project.IsActive);

            var totalScans = await _qrCodeScanRepository.CountAsync(x => x.UserId == personnelId);

            var scanDates = await _qrCodeScanRepository.GetAll()
                .Where(x => x.UserId == personnelId)
                .Select(x => x.ScanDate.Date)
                .Distinct()
                .CountAsync();

            var lastActivity = await _qrCodeScanRepository.GetAll()
                .Where(x => x.UserId == personnelId)
                .OrderByDescending(x => x.ScanDate)
                .Select(x => x.ScanDate)
                .FirstOrDefaultAsync();

            // Calculate working hours (simplified)
            var checkIns = await _qrCodeScanRepository.GetAll()
                .Where(x => x.UserId == personnelId && x.ScanType == ScanType.CheckIn)
                .CountAsync();

            var totalWorkingHours = checkIns * 8; // Assume 8 hours per day

            return new PersonnelStatsDto
            {
                PersonnelId = personnelId,
                PersonnelName = $"{user.FirstName} {user.LastName}",
                TotalProjects = totalProjects,
                ActiveProjects = activeProjects,
                TotalScans = totalScans,
                TotalWorkDays = scanDates,
                LastActivity = lastActivity,
                TotalWorkingHours = totalWorkingHours,
                Role = user.Role.ToString(),
                LastLogin = user.LastLoginAt
            };
        }

        public async Task<GeneralPersonnelStatsDto> GetGeneralPersonnelStatsAsync(Guid? companyId = null)
        {
            var query = _userRepository.GetAll();

            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyId == companyId);
            }

            var totalPersonnel = await query.CountAsync();
            var activePersonnel = await query.CountAsync(x => x.IsActive);
            var inactivePersonnel = totalPersonnel - activePersonnel;

            var todayCheckins = await _qrCodeScanRepository.CountAsync(x => 
                x.ScanDate.Date == DateTime.Today && 
                x.ScanType == ScanType.CheckIn &&
                (companyId == null || x.User.CompanyId == companyId));

            var adminCount = await query.CountAsync(x => x.Role == UserRoleType.Admin);
            var officeStaffCount = await query.CountAsync(x => x.Role == UserRoleType.OfficeStaff);
            var siteStaffCount = await query.CountAsync(x => x.Role == UserRoleType.SiteStaff);
            var subcontractorCount = await query.CountAsync(x => x.Role == UserRoleType.Subcontractor);

            var personnelByCompany = await _userRepository.GetAll()
                .Include(x => x.Company)
                .Where(x => x.CompanyId.HasValue)
                .GroupBy(x => new { x.CompanyId, x.Company.Name })
                .Select(g => new PersonnelByCompanyDto
                {
                    CompanyId = g.Key.CompanyId.Value,
                    CompanyName = g.Key.Name,
                    PersonnelCount = g.Count()
                })
                .ToListAsync();

            return new GeneralPersonnelStatsDto
            {
                TotalPersonnel = totalPersonnel,
                ActivePersonnel = activePersonnel,
                InactivePersonnel = inactivePersonnel,
                TodayCheckins = todayCheckins,
                AdminCount = adminCount,
                OfficeStaffCount = officeStaffCount,
                SiteStaffCount = siteStaffCount,
                SubcontractorCount = subcontractorCount,
                PersonnelByCompany = personnelByCompany
            };
        }

        public async Task<List<ConstructionTrackerUserDto>> GetActivePersonnelAsync()
        {
            var currentUser = await GetCurrentUserAsync();
            var query = _userRepository.GetAll()
                .Include(x => x.Company)
                .Where(x => x.IsActive);

            // Apply company filter for non-admin users
            if (currentUser.Role != UserRoleType.Admin && currentUser.CompanyId.HasValue)
            {
                query = query.Where(x => x.CompanyId == currentUser.CompanyId);
            }

            var users = await query.OrderBy(x => x.FirstName).ToListAsync();
            var dtos = new List<ConstructionTrackerUserDto>();

            foreach (var user in users)
            {
                var dto = ObjectMapper.Map<ConstructionTrackerUserDto>(user);
                dto.CompanyName = user.Company?.Name;
                dtos.Add(dto);
            }

            return dtos;
        }

        public async Task<List<ConstructionTrackerUserDto>> GetCompanyPersonnelAsync(Guid companyId)
        {
            var users = await _userRepository.GetAll()
                .Include(x => x.Company)
                .Where(x => x.CompanyId == companyId && x.IsActive)
                .OrderBy(x => x.FirstName)
                .ToListAsync();

            var dtos = new List<ConstructionTrackerUserDto>();
            foreach (var user in users)
            {
                var dto = ObjectMapper.Map<ConstructionTrackerUserDto>(user);
                dto.CompanyName = user.Company?.Name;
                dtos.Add(dto);
            }

            return dtos;
        }

        public async Task<List<ConstructionTrackerUserDto>> GetProjectPersonnelAsync(Guid projectId)
        {
            var users = await _userProjectRepository.GetAll()
                .Include(x => x.User)
                .ThenInclude(x => x.Company)
                .Where(x => x.ProjectId == projectId && x.IsActive && x.User.IsActive)
                .Select(x => x.User)
                .OrderBy(x => x.FirstName)
                .ToListAsync();

            var dtos = new List<ConstructionTrackerUserDto>();
            foreach (var user in users)
            {
                var dto = ObjectMapper.Map<ConstructionTrackerUserDto>(user);
                dto.CompanyName = user.Company?.Name;
                dtos.Add(dto);
            }

            return dtos;
        }

        public async Task ResetPersonnelPasswordAsync(long personnelId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(personnelId.ToString());
            if (user == null)
                throw new Abp.UI.UserFriendlyException("Personel bulunamadı");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
            {
                throw new Abp.UI.UserFriendlyException("Şifre sıfırlama işlemi başarısız: " + 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        public async Task AssignPersonnelToProjectAsync(long personnelId, Guid projectId, string role)
        {
            // Check if already assigned
            var existing = await _userProjectRepository.FirstOrDefaultAsync(x => 
                x.UserId == personnelId && x.ProjectId == projectId);

            if (existing != null)
            {
                existing.IsActive = true;
                existing.Role = role;
                await _userProjectRepository.UpdateAsync(existing);
            }
            else
            {
                var userProject = new UserProject
                {
                    UserId = personnelId,
                    ProjectId = projectId,
                    Role = role,
                    IsActive = true,
                    AssignedDate = DateTime.Now
                };

                await _userProjectRepository.InsertAsync(userProject);
            }
        }

        public async Task RemovePersonnelFromProjectAsync(long personnelId, Guid projectId)
        {
            var userProject = await _userProjectRepository.FirstOrDefaultAsync(x => 
                x.UserId == personnelId && x.ProjectId == projectId);

            if (userProject != null)
            {
                userProject.IsActive = false;
                await _userProjectRepository.UpdateAsync(userProject);
            }
        }

        private async Task<User> GetCurrentUserAsync()
        {
            var userId = AbpSession.GetUserId();
            return await _userRepository.GetAsync(userId);
        }
    }
}
