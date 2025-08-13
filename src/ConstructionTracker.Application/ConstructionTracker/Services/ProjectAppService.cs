using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using ConstructionTracker.Authorization.Users;
using ConstructionTracker.ConstructionTracker.Dto;
using ConstructionTracker.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConstructionTracker.ConstructionTracker.Services
{
    [AbpAuthorize]
    public class ProjectAppService : ConstructionTrackerAppServiceBase, IProjectAppService
    {
        private readonly IRepository<Project, Guid> _projectRepository;
        private readonly IRepository<UserProject, Guid> _userProjectRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<ActivityLog, Guid> _activityLogRepository;

        public ProjectAppService(
            IRepository<Project, Guid> projectRepository,
            IRepository<UserProject, Guid> userProjectRepository,
            IRepository<User, long> userRepository,
            IRepository<ActivityLog, Guid> activityLogRepository)
        {
            _projectRepository = projectRepository;
            _userProjectRepository = userProjectRepository;
            _userRepository = userRepository;
            _activityLogRepository = activityLogRepository;
        }

        public async Task<ProjectDto> GetAsync(Guid id)
        {
            var project = await _projectRepository.GetAll()
                .Include(x => x.Company)
                .Include(x => x.UserProjects)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            var projectDto = ObjectMapper.Map<ProjectDto>(project);
            
            if (project?.Company != null)
            {
                projectDto.CompanyName = project.Company.Name;
            }

            if (project?.UserProjects != null)
            {
                projectDto.AssignedUsers = ObjectMapper.Map<List<ConstructionTrackerUserDto>>(
                    project.UserProjects.Where(x => x.IsActive).Select(x => x.User).ToList());
            }

            return projectDto;
        }

        public async Task<PagedResultDto<ProjectDto>> GetAllAsync(PagedAndSortedResultRequestDto input)
        {
            var query = _projectRepository.GetAll()
                .Include(x => x.Company)
                .Where(x => x.IsActive);

            var totalCount = await query.CountAsync();
            
            var projects = await query
                .OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var projectDtos = ObjectMapper.Map<List<ProjectDto>>(projects);
            
            // Set company names
            foreach (var dto in projectDtos)
            {
                var project = projects.First(x => x.Id == dto.Id);
                if (project.Company != null)
                {
                    dto.CompanyName = project.Company.Name;
                }
            }

            return new PagedResultDto<ProjectDto>(totalCount, projectDtos);
        }

        public async Task<List<ProjectSummaryDto>> GetUserProjectsAsync(long userId)
        {
            var userProjects = await _userProjectRepository.GetAll()
                .Include(x => x.Project)
                .Where(x => x.UserId == userId && x.IsActive && x.Project.IsActive)
                .Select(x => x.Project)
                .ToListAsync();

            return ObjectMapper.Map<List<ProjectSummaryDto>>(userProjects);
        }

        public async Task<ProjectDto> CreateAsync(CreateProjectDto input)
        {
            var project = ObjectMapper.Map<Project>(input);
            project = await _projectRepository.InsertAsync(project);

            // Assign users to project
            if (input.AssignedUserIds != null && input.AssignedUserIds.Any())
            {
                foreach (var userId in input.AssignedUserIds)
                {
                    await _userProjectRepository.InsertAsync(new UserProject
                    {
                        UserId = userId,
                        ProjectId = project.Id,
                        AssignedDate = DateTime.Now,
                        IsActive = true,
                        Role = "Team Member"
                    });
                }
            }

            // Log activity
            await LogActivityAsync(new CreateActivityLogDto
            {
                Title = "Proje Oluşturuldu",
                Description = $"Yeni proje oluşturuldu: {project.Name}",
                ActivityType = ActivityType.ProjectCreated,
                ProjectId = project.Id,
                CompanyId = project.CompanyId,
                ActivityDate = DateTime.Now
            });

            return ObjectMapper.Map<ProjectDto>(project);
        }

        public async Task<ProjectDto> UpdateAsync(UpdateProjectDto input)
        {
            var project = await _projectRepository.GetAsync(input.Id);
            
            ObjectMapper.Map(input, project);
            await _projectRepository.UpdateAsync(project);

            // Log activity
            await LogActivityAsync(new CreateActivityLogDto
            {
                Title = "Proje Güncellendi",
                Description = $"Proje güncellendi: {project.Name}",
                ActivityType = ActivityType.ProjectUpdated,
                ProjectId = project.Id,
                CompanyId = project.CompanyId,
                ActivityDate = DateTime.Now
            });

            return ObjectMapper.Map<ProjectDto>(project);
        }

        public async Task DeleteAsync(Guid id)
        {
            var project = await _projectRepository.GetAsync(id);
            project.IsActive = false;
            await _projectRepository.UpdateAsync(project);
        }

        public async Task<List<ConstructionTrackerUserDto>> GetProjectUsersAsync(Guid projectId)
        {
            var users = await _userProjectRepository.GetAll()
                .Include(x => x.User)
                .Where(x => x.ProjectId == projectId && x.IsActive)
                .Select(x => x.User)
                .ToListAsync();

            return ObjectMapper.Map<List<ConstructionTrackerUserDto>>(users);
        }

        public async Task AssignUserToProjectAsync(Guid projectId, long userId, string role)
        {
            var existingAssignment = await _userProjectRepository.FirstOrDefaultAsync(
                x => x.ProjectId == projectId && x.UserId == userId);

            if (existingAssignment != null)
            {
                existingAssignment.IsActive = true;
                existingAssignment.Role = role;
                existingAssignment.AssignedDate = DateTime.Now;
                await _userProjectRepository.UpdateAsync(existingAssignment);
            }
            else
            {
                await _userProjectRepository.InsertAsync(new UserProject
                {
                    UserId = userId,
                    ProjectId = projectId,
                    Role = role,
                    AssignedDate = DateTime.Now,
                    IsActive = true
                });
            }

            var user = await _userRepository.GetAsync(userId);
            var project = await _projectRepository.GetAsync(projectId);

            // Log activity
            await LogActivityAsync(new CreateActivityLogDto
            {
                Title = "Personel Atandı",
                Description = $"{user.FirstName} {user.LastName} projeye atandı: {project.Name}",
                ActivityType = ActivityType.PersonnelAssigned,
                UserId = userId,
                ProjectId = projectId,
                CompanyId = project.CompanyId,
                ActivityDate = DateTime.Now
            });
        }

        public async Task RemoveUserFromProjectAsync(Guid projectId, long userId)
        {
            var assignment = await _userProjectRepository.FirstOrDefaultAsync(
                x => x.ProjectId == projectId && x.UserId == userId);

            if (assignment != null)
            {
                assignment.IsActive = false;
                assignment.UnassignedDate = DateTime.Now;
                await _userProjectRepository.UpdateAsync(assignment);

                var user = await _userRepository.GetAsync(userId);
                var project = await _projectRepository.GetAsync(projectId);

                // Log activity
                await LogActivityAsync(new CreateActivityLogDto
                {
                    Title = "Personel Kaldırıldı",
                    Description = $"{user.FirstName} {user.LastName} projeden kaldırıldı: {project.Name}",
                    ActivityType = ActivityType.PersonnelRemoved,
                    UserId = userId,
                    ProjectId = projectId,
                    CompanyId = project.CompanyId,
                    ActivityDate = DateTime.Now
                });
            }
        }

        public async Task UpdateProjectProgressAsync(Guid projectId, decimal progress)
        {
            var project = await _projectRepository.GetAsync(projectId);
            project.Progress = Math.Max(0, Math.Min(100, progress));
            await _projectRepository.UpdateAsync(project);
        }

        public async Task CompleteProjectAsync(Guid projectId)
        {
            var project = await _projectRepository.GetAsync(projectId);
            project.Status = ProjectStatus.Completed;
            project.Progress = 100;
            project.EndDate = DateTime.Now;
            await _projectRepository.UpdateAsync(project);

            // Log activity
            await LogActivityAsync(new CreateActivityLogDto
            {
                Title = "Proje Tamamlandı",
                Description = $"Proje tamamlandı: {project.Name}",
                ActivityType = ActivityType.ProjectCompleted,
                ProjectId = project.Id,
                CompanyId = project.CompanyId,
                ActivityDate = DateTime.Now
            });
        }

        public async Task<ProjectStatsDto> GetProjectStatsAsync(Guid? companyId = null)
        {
            var query = _projectRepository.GetAll().Where(x => x.IsActive);
            
            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyId == companyId.Value);
            }

            var totalProjects = await query.CountAsync();
            var activeProjects = await query.Where(x => x.Status == ProjectStatus.Active).CountAsync();
            var completedProjects = await query.Where(x => x.Status == ProjectStatus.Completed).CountAsync();
            var delayedProjects = await query.Where(x => x.Status == ProjectStatus.Delayed).CountAsync();
            
            var totalBudget = await query.Where(x => x.Budget.HasValue).SumAsync(x => x.Budget.Value);
            var totalSpent = await query.Where(x => x.SpentAmount.HasValue).SumAsync(x => x.SpentAmount.Value);
            var averageProgress = await query.AverageAsync(x => x.Progress);

            return new ProjectStatsDto
            {
                TotalProjects = totalProjects,
                ActiveProjects = activeProjects,
                CompletedProjects = completedProjects,
                DelayedProjects = delayedProjects,
                TotalBudget = totalBudget,
                TotalSpent = totalSpent,
                AverageProgress = averageProgress
            };
        }

        public async Task<List<ProjectSummaryDto>> GetActiveProjectsAsync()
        {
            var projects = await _projectRepository.GetAll()
                .Where(x => x.IsActive && x.Status == ProjectStatus.Active)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();

            return ObjectMapper.Map<List<ProjectSummaryDto>>(projects);
        }

        public async Task<List<ProjectSummaryDto>> GetDelayedProjectsAsync()
        {
            var today = DateTime.Today;
            var projects = await _projectRepository.GetAll()
                .Where(x => x.IsActive && 
                           (x.Status == ProjectStatus.Delayed || 
                            (x.PlannedEndDate.HasValue && x.PlannedEndDate.Value < today && x.Status != ProjectStatus.Completed)))
                .OrderBy(x => x.PlannedEndDate)
                .ToListAsync();

            return ObjectMapper.Map<List<ProjectSummaryDto>>(projects);
        }

        private async Task LogActivityAsync(CreateActivityLogDto input)
        {
            var activity = ObjectMapper.Map<ActivityLog>(input);
            await _activityLogRepository.InsertAsync(activity);
        }
    }
} 