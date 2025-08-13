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
    public class PersonnelController : AbpController
    {
        private readonly IPersonnelAppService _personnelAppService;

        public PersonnelController(IPersonnelAppService personnelAppService)
        {
            _personnelAppService = personnelAppService;
        }

        /// <summary>
        /// Tüm personelleri sayfalı olarak getir
        /// </summary>
        [HttpGet]
        public async Task<PagedResultDto<ConstructionTrackerUserDto>> GetAll([FromQuery] PagedPersonnelRequestDto input)
        {
            return await _personnelAppService.GetAllPersonnelAsync(input);
        }

        /// <summary>
        /// Personel detayını getir
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ConstructionTrackerUserDto> Get(long id)
        {
            return await _personnelAppService.GetPersonnelAsync(id);
        }

        /// <summary>
        /// Yeni personel ekle
        /// </summary>
        [HttpPost]
        public async Task<ConstructionTrackerUserDto> Create([FromBody] CreatePersonnelDto input)
        {
            return await _personnelAppService.CreatePersonnelAsync(input);
        }

        /// <summary>
        /// Personel bilgilerini güncelle
        /// </summary>
        [HttpPut]
        public async Task<ConstructionTrackerUserDto> Update([FromBody] UpdatePersonnelDto input)
        {
            return await _personnelAppService.UpdatePersonnelAsync(input);
        }

        /// <summary>
        /// Personeli sil (deaktive et)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _personnelAppService.DeletePersonnelAsync(id);
            return Ok();
        }

        /// <summary>
        /// Personelin çalıştığı projeleri getir
        /// </summary>
        [HttpGet("{id}/projects")]
        public async Task<List<ProjectSummaryDto>> GetPersonnelProjects(long id)
        {
            return await _personnelAppService.GetPersonnelProjectsAsync(id);
        }

        /// <summary>
        /// Personelin QR kod tarama geçmişini getir
        /// </summary>
        [HttpGet("{id}/scans")]
        public async Task<PagedResultDto<QrCodeScanDto>> GetPersonnelScans(
            long id, 
            [FromQuery] PagedAndSortedResultRequestDto input)
        {
            return await _personnelAppService.GetPersonnelScansAsync(id, input);
        }

        /// <summary>
        /// Personel istatistiklerini getir
        /// </summary>
        [HttpGet("{id}/stats")]
        public async Task<PersonnelStatsDto> GetPersonnelStats(long id)
        {
            return await _personnelAppService.GetPersonnelStatsAsync(id);
        }

        /// <summary>
        /// Aktif personelleri getir
        /// </summary>
        [HttpGet("active")]
        public async Task<List<ConstructionTrackerUserDto>> GetActivePersonnel()
        {
            return await _personnelAppService.GetActivePersonnelAsync();
        }

        /// <summary>
        /// Şirket personellerini getir
        /// </summary>
        [HttpGet("company/{companyId}")]
        public async Task<List<ConstructionTrackerUserDto>> GetCompanyPersonnel(Guid companyId)
        {
            return await _personnelAppService.GetCompanyPersonnelAsync(companyId);
        }

        /// <summary>
        /// Proje personellerini getir
        /// </summary>
        [HttpGet("project/{projectId}")]
        public async Task<List<ConstructionTrackerUserDto>> GetProjectPersonnel(Guid projectId)
        {
            return await _personnelAppService.GetProjectPersonnelAsync(projectId);
        }

        /// <summary>
        /// Personelin şifrini sıfırla
        /// </summary>
        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(long id, [FromBody] ResetPasswordRequestDto input)
        {
            await _personnelAppService.ResetPersonnelPasswordAsync(id, input.NewPassword);
            return Ok(new { message = "Şifre başarıyla sıfırlandı" });
        }

        /// <summary>
        /// Personeli projeye ata
        /// </summary>
        [HttpPost("{personnelId}/assign-project")]
        public async Task<IActionResult> AssignToProject(long personnelId, [FromBody] AssignPersonnelToProjectRequestDto input)
        {
            await _personnelAppService.AssignPersonnelToProjectAsync(personnelId, input.ProjectId, input.Role);
            return Ok();
        }

        /// <summary>
        /// Personeli projeden kaldır
        /// </summary>
        [HttpDelete("{personnelId}/projects/{projectId}")]
        public async Task<IActionResult> RemoveFromProject(long personnelId, Guid projectId)
        {
            await _personnelAppService.RemovePersonnelFromProjectAsync(personnelId, projectId);
            return Ok();
        }

        /// <summary>
        /// Genel personel istatistiklerini getir
        /// </summary>
        [HttpGet("stats")]
        public async Task<GeneralPersonnelStatsDto> GetGeneralStats([FromQuery] Guid? companyId = null)
        {
            return await _personnelAppService.GetGeneralPersonnelStatsAsync(companyId);
        }
    }

    // Helper DTOs - kept in controller for simple request models
    public class AssignPersonnelToProjectRequestDto
    {
        public Guid ProjectId { get; set; }
        public string Role { get; set; }
    }

    public class ResetPasswordRequestDto
    {
        public string NewPassword { get; set; }
    }
}
