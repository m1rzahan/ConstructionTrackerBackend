using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Controllers;
using ConstructionTracker.ConstructionTracker.Dto;
using ConstructionTracker.ConstructionTracker.Services;
using ConstructionTracker.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskStatus = ConstructionTracker.Entities.TaskStatus;

namespace ConstructionTracker.Web.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : AbpController
    {
        private readonly ITaskAppService _taskAppService;

        public TasksController(ITaskAppService taskAppService)
        {
            _taskAppService = taskAppService;
            Console.WriteLine("🔧 TasksController constructor called");
        }

        /// <summary>
        /// Tüm görevleri sayfalı olarak getir
        /// </summary>
        [HttpGet("list")]
        public async Task<PagedResultDto<TaskDto>> GetAll([FromQuery] PagedTaskRequestDto input)
        {
            return await _taskAppService.GetAllAsync(input);
        }

        /// <summary>
        /// Görev detayını getir
        /// </summary>
        [HttpGet("{id}")]
        public async Task<TaskDto> Get(Guid id)
        {
            return await _taskAppService.GetAsync(id);
        }

        /// <summary>
        /// Test endpoint
        /// </summary>
        [HttpPost("test")]
        public IActionResult Test()
        {
            Console.WriteLine("🧪 TasksController.Test method called");
            return Ok(new { message = "TasksController is working!", timestamp = DateTime.Now });
        }

        /// <summary>
        /// Yeni görev oluştur
        /// </summary>
        [HttpPost("create")]
        public async Task<TaskDto> Create([FromBody] CreateTaskDto input)
        {
            try
            {
                Console.WriteLine("🎯 TasksController.Create method called");
                Console.WriteLine($"📥 Input: {System.Text.Json.JsonSerializer.Serialize(input)}");
                
                if (input == null)
                {
                    Console.WriteLine("❌ Input is null!");
                    throw new ArgumentNullException(nameof(input));
                }
                
                Console.WriteLine($"🔍 Validating input: Title='{input.Title}', ProjectId='{input.ProjectId}', AssignedToUserId='{input.AssignedToUserId}'");
                
                var result = await _taskAppService.CreateAsync(input);
                Console.WriteLine($"✅ Task created successfully: {result.Id}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR in Create method: {ex.Message}");
                Console.WriteLine($"💥 Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Görev güncelle
        /// </summary>
        [HttpPut]
        public async Task<TaskDto> Update([FromBody] UpdateTaskDto input)
        {
            return await _taskAppService.UpdateAsync(input);
        }

        /// <summary>
        /// Görev sil
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _taskAppService.DeleteAsync(id);
            return Ok();
        }

        /// <summary>
        /// Görev durumunu güncelle
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<TaskDto> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusDto input)
        {
            return await _taskAppService.UpdateStatusAsync(id, input);
        }

        /// <summary>
        /// Görevi tamamla
        /// </summary>
        [HttpPost("{id}/complete")]
        public async Task<TaskDto> Complete(Guid id, [FromBody] CompleteTaskRequestDto input)
        {
            return await _taskAppService.CompleteTaskAsync(id, input.Comment);
        }

        /// <summary>
        /// Görevi kullanıcıya ata
        /// </summary>
        [HttpPost("{id}/assign")]
        public async Task<TaskDto> AssignTask(Guid id, [FromBody] AssignTaskRequestDto input)
        {
            return await _taskAppService.AssignTaskAsync(id, input.UserId);
        }

        /// <summary>
        /// Kullanıcının görevlerini getir
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<List<TaskSummaryDto>> GetUserTasks(long userId)
        {
            return await _taskAppService.GetUserTasksAsync(userId);
        }

        /// <summary>
        /// Projeye ait görevleri getir
        /// </summary>
        [HttpGet("project/{projectId}")]
        public async Task<List<TaskSummaryDto>> GetProjectTasks(Guid projectId)
        {
            return await _taskAppService.GetProjectTasksAsync(projectId);
        }

        /// <summary>
        /// Duruma göre görevleri getir
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<List<TaskSummaryDto>> GetTasksByStatus(TaskStatus status)
        {
            return await _taskAppService.GetTasksByStatusAsync(status);
        }

        /// <summary>
        /// Geciken görevleri getir
        /// </summary>
        [HttpGet("overdue")]
        public async Task<List<TaskSummaryDto>> GetOverdueTasks()
        {
            return await _taskAppService.GetOverdueTasksAsync();
        }

        /// <summary>
        /// Son görevleri getir
        /// </summary>
        [HttpGet("recent")]
        public async Task<List<TaskSummaryDto>> GetRecentTasks([FromQuery] int count = 10)
        {
            return await _taskAppService.GetRecentTasksAsync(count);
        }

        /// <summary>
        /// Önceliğe göre görevleri getir
        /// </summary>
        [HttpGet("priority/{priority}")]
        public async Task<List<TaskSummaryDto>> GetTasksByPriority(TaskPriority priority)
        {
            return await _taskAppService.GetTasksByPriorityAsync(priority);
        }

        /// <summary>
        /// Görevlerde arama yap
        /// </summary>
        [HttpGet("search")]
        public async Task<PagedResultDto<TaskDto>> SearchTasks([FromQuery] string searchTerm, [FromQuery] PagedAndSortedResultRequestDto input)
        {
            return await _taskAppService.SearchTasksAsync(searchTerm, input);
        }

        /// <summary>
        /// Görev istatistiklerini getir
        /// </summary>
        [HttpGet("stats")]
        public async Task<TaskStatsDto> GetTaskStats([FromQuery] Guid? projectId = null, [FromQuery] long? userId = null)
        {
            return await _taskAppService.GetTaskStatsAsync(projectId, userId);
        }

        /// <summary>
        /// Şirket görev istatistiklerini getir
        /// </summary>
        [HttpGet("company-stats")]
        public async Task<TaskStatsDto> GetCompanyTaskStats([FromQuery] Guid? companyId = null)
        {
            return await _taskAppService.GetCompanyTaskStatsAsync(companyId);
        }

        /// <summary>
        /// Göreve yorum ekle
        /// </summary>
        [HttpPost("{taskId}/comments")]
        public async Task<TaskCommentDto> AddComment(Guid taskId, [FromBody] AddCommentRequestDto input)
        {
            return await _taskAppService.AddCommentAsync(new CreateTaskCommentDto 
            { 
                TaskId = taskId, 
                Comment = input.Comment 
            });
        }

        /// <summary>
        /// Görev yorumlarını getir
        /// </summary>
        [HttpGet("{taskId}/comments")]
        public async Task<List<TaskCommentDto>> GetTaskComments(Guid taskId)
        {
            return await _taskAppService.GetTaskCommentsAsync(taskId);
        }

        /// <summary>
        /// Yorum sil
        /// </summary>
        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            await _taskAppService.DeleteCommentAsync(commentId);
            return Ok();
        }

        /// <summary>
        /// Göreve fotoğraf ekle
        /// </summary>
        [HttpPost("{taskId}/photos")]
        public async Task<TaskPhotoDto> AddPhoto(Guid taskId, [FromBody] AddPhotoRequestDto input)
        {
            return await _taskAppService.AddPhotoAsync(new CreateTaskPhotoDto 
            { 
                TaskId = taskId,
                FilePath = input.FilePath,
                FileName = input.FileName,
                Description = input.Description,
                FileSize = input.FileSize,
                ContentType = input.ContentType
            });
        }

        /// <summary>
        /// Görev fotoğraflarını getir
        /// </summary>
        [HttpGet("{taskId}/photos")]
        public async Task<List<TaskPhotoDto>> GetTaskPhotos(Guid taskId)
        {
            return await _taskAppService.GetTaskPhotosAsync(taskId);
        }

        /// <summary>
        /// Fotoğraf sil
        /// </summary>
        [HttpDelete("photos/{photoId}")]
        public async Task<IActionResult> DeletePhoto(Guid photoId)
        {
            await _taskAppService.DeletePhotoAsync(photoId);
            return Ok();
        }
    }

    // Helper DTOs for controller requests
    public class CompleteTaskRequestDto
    {
        public string Comment { get; set; }
    }

    public class AssignTaskRequestDto
    {
        public long UserId { get; set; }
    }

    public class AddCommentRequestDto
    {
        public string Comment { get; set; }
    }

    public class AddPhotoRequestDto
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public long? FileSize { get; set; }
        public string ContentType { get; set; }
    }
}
