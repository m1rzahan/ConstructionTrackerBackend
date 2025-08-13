using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Controllers;
using ConstructionTracker.ConstructionTracker.Dto;
using ConstructionTracker.ConstructionTracker.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConstructionTracker.Web.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : AbpController
    {
        private readonly IProjectAppService _projectAppService;

        public ProjectsController(IProjectAppService projectAppService)
        {
            _projectAppService = projectAppService;
        }

        /// <summary>
        /// Proje detayını getir
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ProjectDto> Get(Guid id)
        {
            return await _projectAppService.GetAsync(id);
        }

        /// <summary>
        /// Tüm projeleri sayfalı olarak getir
        /// </summary>
        [HttpGet]
        public async Task<PagedResultDto<ProjectDto>> GetAll([FromQuery] PagedAndSortedResultRequestDto input)
        {
            return await _projectAppService.GetAllAsync(input);
        }

        /// <summary>
        /// Kullanıcının atandığı projeleri getir
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<List<ProjectSummaryDto>> GetUserProjects(long userId)
        {
            return await _projectAppService.GetUserProjectsAsync(userId);
        }

        /// <summary>
        /// Yeni proje oluştur
        /// </summary>
        [HttpPost]
        public async Task<ProjectDto> Create([FromBody] CreateProjectDto input)
        {
            return await _projectAppService.CreateAsync(input);
        }

        /// <summary>
        /// Proje güncelle
        /// </summary>
        [HttpPut]
        public async Task<ProjectDto> Update([FromBody] UpdateProjectDto input)
        {
            return await _projectAppService.UpdateAsync(input);
        }

        /// <summary>
        /// Proje sil
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _projectAppService.DeleteAsync(id);
            return Ok();
        }

        /// <summary>
        /// Projenin personellerini getir
        /// </summary>
        [HttpGet("{projectId}/users")]
        public async Task<List<ConstructionTrackerUserDto>> GetProjectUsers(Guid projectId)
        {
            return await _projectAppService.GetProjectUsersAsync(projectId);
        }

        /// <summary>
        /// Projeye kullanıcı ata
        /// </summary>
        [HttpPost("{projectId}/assign-user")]
        public async Task<IActionResult> AssignUserToProject(Guid projectId, [FromBody] AssignUserToProjectRequestDto input)
        {
            await _projectAppService.AssignUserToProjectAsync(projectId, input.UserId, input.Role);
            return Ok();
        }

        /// <summary>
        /// Projeden kullanıcı kaldır
        /// </summary>
        [HttpDelete("{projectId}/users/{userId}")]
        public async Task<IActionResult> RemoveUserFromProject(Guid projectId, long userId)
        {
            await _projectAppService.RemoveUserFromProjectAsync(projectId, userId);
            return Ok();
        }

        /// <summary>
        /// Proje ilerlemesini güncelle
        /// </summary>
        [HttpPut("{projectId}/progress")]
        public async Task<IActionResult> UpdateProgress(Guid projectId, [FromBody] UpdateProgressRequestDto input)
        {
            await _projectAppService.UpdateProjectProgressAsync(projectId, input.Progress);
            return Ok();
        }

        /// <summary>
        /// Projeyi tamamla
        /// </summary>
        [HttpPost("{projectId}/complete")]
        public async Task<IActionResult> CompleteProject(Guid projectId)
        {
            await _projectAppService.CompleteProjectAsync(projectId);
            return Ok();
        }

        /// <summary>
        /// Proje istatistiklerini getir
        /// </summary>
        [HttpGet("stats")]
        public async Task<ProjectStatsDto> GetProjectStats([FromQuery] Guid? companyId = null)
        {
            return await _projectAppService.GetProjectStatsAsync(companyId);
        }

        /// <summary>
        /// Aktif projeleri getir
        /// </summary>
        [HttpGet("active")]
        public async Task<List<ProjectSummaryDto>> GetActiveProjects()
        {
            return await _projectAppService.GetActiveProjectsAsync();
        }

        /// <summary>
        /// Geciken projeleri getir
        /// </summary>
        [HttpGet("delayed")]
        public async Task<List<ProjectSummaryDto>> GetDelayedProjects()
        {
            return await _projectAppService.GetDelayedProjectsAsync();
        }
    }

    // Helper DTOs for requests
    public class AssignUserToProjectRequestDto
    {
        public long UserId { get; set; }
        public string Role { get; set; }
    }

    public class UpdateProgressRequestDto
    {
        public decimal Progress { get; set; }
    }
} 