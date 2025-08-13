using Abp.Application.Services;
using Abp.Application.Services.Dto;
using ConstructionTracker.ConstructionTracker.Dto;
using ConstructionTracker.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskStatus = ConstructionTracker.Entities.TaskStatus;

namespace ConstructionTracker.ConstructionTracker.Services
{
    public interface ITaskAppService : IApplicationService
    {
        // CRUD Operations
        Task<TaskDto> GetAsync(Guid id);
        Task<PagedResultDto<TaskDto>> GetAllAsync(PagedTaskRequestDto input);
        Task<TaskDto> CreateAsync(CreateTaskDto input);
        Task<TaskDto> UpdateAsync(UpdateTaskDto input);
        Task DeleteAsync(Guid id);

        // Task Management
        Task<TaskDto> UpdateStatusAsync(Guid taskId, UpdateTaskStatusDto input);
        Task<TaskDto> CompleteTaskAsync(Guid taskId, string comment = null);
        Task<TaskDto> AssignTaskAsync(Guid taskId, long userId);

        // Task Queries
        Task<List<TaskSummaryDto>> GetUserTasksAsync(long userId);
        Task<List<TaskSummaryDto>> GetProjectTasksAsync(Guid projectId);
        Task<List<TaskSummaryDto>> GetTasksByStatusAsync(TaskStatus status);
        Task<List<TaskSummaryDto>> GetOverdueTasksAsync();
        Task<List<TaskSummaryDto>> GetRecentTasksAsync(int count = 10);

        // Comments
        Task<TaskCommentDto> AddCommentAsync(CreateTaskCommentDto input);
        Task<List<TaskCommentDto>> GetTaskCommentsAsync(Guid taskId);
        Task DeleteCommentAsync(Guid commentId);

        // Photos
        Task<TaskPhotoDto> AddPhotoAsync(CreateTaskPhotoDto input);
        Task<List<TaskPhotoDto>> GetTaskPhotosAsync(Guid taskId);
        Task DeletePhotoAsync(Guid photoId);

        // Statistics
        Task<TaskStatsDto> GetTaskStatsAsync(Guid? projectId = null, long? userId = null);
        Task<TaskStatsDto> GetCompanyTaskStatsAsync(Guid? companyId = null);

        // Search and Filter
        Task<PagedResultDto<TaskDto>> SearchTasksAsync(string searchTerm, PagedAndSortedResultRequestDto input);
        Task<List<TaskSummaryDto>> GetTasksByPriorityAsync(TaskPriority priority);
    }
}
