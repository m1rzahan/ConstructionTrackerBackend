using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using ConstructionTracker.Entities;
using System;
using System.Collections.Generic;

namespace ConstructionTracker.ConstructionTracker.Dto
{
    [AutoMapFrom(typeof(ProjectTask))]
    public class TaskDto : EntityDto<Guid>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; }
        public long AssignedToUserId { get; set; }
        public string AssignedToUserName { get; set; }
        public long AssignedByUserId { get; set; }
        public string AssignedByUserName { get; set; }
        public DateTime? DueDate { get; set; }
        public string Location { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Notes { get; set; }
        public DateTime CreationTime { get; set; }
        public int PhotoCount { get; set; }
        public int CommentCount { get; set; }
        public List<TaskCommentDto> Comments { get; set; }
        public List<TaskPhotoDto> Photos { get; set; }

        public TaskDto()
        {
            Comments = new List<TaskCommentDto>();
            Photos = new List<TaskPhotoDto>();
        }
    }

    public class CreateTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskPriority Priority { get; set; }
        public Guid ProjectId { get; set; }
        public long AssignedToUserId { get; set; }
        public DateTime? DueDate { get; set; }
        public string Location { get; set; }
        public string Notes { get; set; }
    }

    public class UpdateTaskDto : EntityDto<Guid>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public long AssignedToUserId { get; set; }
        public DateTime? DueDate { get; set; }
        public string Location { get; set; }
        public string Notes { get; set; }
    }

    public class UpdateTaskStatusDto
    {
        public TaskStatus Status { get; set; }
        public string Comment { get; set; }
    }

    public class TaskSummaryDto : EntityDto<Guid>
    {
        public string Title { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public string ProjectName { get; set; }
        public string AssignedToUserName { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreationTime { get; set; }
        public int PhotoCount { get; set; }
        public int CommentCount { get; set; }
    }

    [AutoMapFrom(typeof(TaskComment))]
    public class TaskCommentDto : EntityDto<Guid>
    {
        public Guid TaskId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string Comment { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class CreateTaskCommentDto
    {
        public Guid TaskId { get; set; }
        public string Comment { get; set; }
    }

    [AutoMapFrom(typeof(TaskPhoto))]
    public class TaskPhotoDto : EntityDto<Guid>
    {
        public Guid TaskId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public long? FileSize { get; set; }
        public string ContentType { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class CreateTaskPhotoDto
    {
        public Guid TaskId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public long? FileSize { get; set; }
        public string ContentType { get; set; }
    }

    public class TaskStatsDto
    {
        public int TotalTasks { get; set; }
        public int TodoTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int HighPriorityTasks { get; set; }
        public double CompletionRate { get; set; }
        public List<TaskSummaryDto> RecentTasks { get; set; }

        public TaskStatsDto()
        {
            RecentTasks = new List<TaskSummaryDto>();
        }
    }

    public class PagedTaskRequestDto : PagedAndSortedResultRequestDto
    {
        public TaskStatus? Status { get; set; }
        public TaskPriority? Priority { get; set; }
        public Guid? ProjectId { get; set; }
        public long? AssignedToUserId { get; set; }
        public string SearchTerm { get; set; }
        public bool? IsOverdue { get; set; }
    }
}
