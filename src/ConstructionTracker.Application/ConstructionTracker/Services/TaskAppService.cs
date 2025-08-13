using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using ConstructionTracker.Authorization.Users;
using ConstructionTracker.ConstructionTracker.Dto;
using ConstructionTracker.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskStatus = ConstructionTracker.Entities.TaskStatus;

namespace ConstructionTracker.ConstructionTracker.Services
{
    [AbpAuthorize]
    public class TaskAppService : ConstructionTrackerAppServiceBase, ITaskAppService
    {
        private readonly IRepository<ProjectTask, Guid> _taskRepository;
        private readonly IRepository<TaskComment, Guid> _taskCommentRepository;
        private readonly IRepository<TaskPhoto, Guid> _taskPhotoRepository;
        private readonly IRepository<Project, Guid> _projectRepository;
        private readonly IRepository<User, long> _userRepository;

        public TaskAppService(
            IRepository<ProjectTask, Guid> taskRepository,
            IRepository<TaskComment, Guid> taskCommentRepository,
            IRepository<TaskPhoto, Guid> taskPhotoRepository,
            IRepository<Project, Guid> projectRepository,
            IRepository<User, long> userRepository)
        {
            _taskRepository = taskRepository;
            _taskCommentRepository = taskCommentRepository;
            _taskPhotoRepository = taskPhotoRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
        }

        public async Task<TaskDto> GetAsync(Guid id)
        {
            var task = await _taskRepository.GetAll()
                .Include(x => x.Project)
                .Include(x => x.AssignedToUser)
                .Include(x => x.AssignedByUser)
                .Include(x => x.Comments)
                .ThenInclude(x => x.User)
                .Include(x => x.Photos)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            var taskDto = ObjectMapper.Map<TaskDto>(task);
            
            // Map related data
            if (task.Project != null)
                taskDto.ProjectName = task.Project.Name;
            
            if (task.AssignedToUser != null)
                taskDto.AssignedToUserName = $"{task.AssignedToUser.FirstName} {task.AssignedToUser.LastName}";
                
            if (task.AssignedByUser != null)
                taskDto.AssignedByUserName = $"{task.AssignedByUser.FirstName} {task.AssignedByUser.LastName}";

            taskDto.PhotoCount = task.Photos?.Count ?? 0;
            taskDto.CommentCount = task.Comments?.Count ?? 0;

            return taskDto;
        }

        public async Task<PagedResultDto<TaskDto>> GetAllAsync(PagedTaskRequestDto input)
        {
            // Default pagination değerleri
            if (input.MaxResultCount <= 0)
                input.MaxResultCount = 100; // Default 100 kayıt
            if (input.SkipCount < 0)
                input.SkipCount = 0;

            var query = _taskRepository.GetAll()
                .Include(x => x.Project)
                .Include(x => x.AssignedToUser)
                .Include(x => x.AssignedByUser)
                .Include(x => x.Comments)
                .Include(x => x.Photos)
                .WhereIf(!string.IsNullOrEmpty(input.SearchTerm), x => 
                    x.Title.Contains(input.SearchTerm) || 
                    x.Description.Contains(input.SearchTerm))
                .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
                .WhereIf(input.Priority.HasValue, x => x.Priority == input.Priority.Value)
                .WhereIf(input.ProjectId.HasValue, x => x.ProjectId == input.ProjectId.Value)
                .WhereIf(input.AssignedToUserId.HasValue, x => x.AssignedToUserId == input.AssignedToUserId.Value)
                .WhereIf(input.IsOverdue.HasValue && input.IsOverdue.Value, x => 
                    x.DueDate.HasValue && x.DueDate.Value < DateTime.Now && x.Status != TaskStatus.Completed);

            var totalCount = await query.CountAsync();
            
            // Eğer veri yoksa test verisi oluştur
            if (totalCount == 0)
            {
                await CreateSampleTasks();
                // Tekrar say
                totalCount = await query.CountAsync();
            }

            var tasks = await query
                .OrderByDescending(x => x.CreationTime)
                .PageBy(input)
                .ToListAsync();

            var taskDtos = new List<TaskDto>();
            foreach (var task in tasks)
            {
                var taskDto = ObjectMapper.Map<TaskDto>(task);
                
                // Map related data
                if (task.Project != null)
                    taskDto.ProjectName = task.Project.Name;
                
                if (task.AssignedToUser != null)
                    taskDto.AssignedToUserName = $"{task.AssignedToUser.FirstName} {task.AssignedToUser.LastName}";
                    
                if (task.AssignedByUser != null)
                    taskDto.AssignedByUserName = $"{task.AssignedByUser.FirstName} {task.AssignedByUser.LastName}";

                taskDto.PhotoCount = task.Photos?.Count ?? 0;
                taskDto.CommentCount = task.Comments?.Count ?? 0;

                taskDtos.Add(taskDto);
            }

            return new PagedResultDto<TaskDto>(totalCount, taskDtos);
        }

        public async Task<TaskDto> CreateAsync(CreateTaskDto input)
        {
            try
            {
                Console.WriteLine($"🔧 TaskAppService.CreateAsync called with input: {System.Text.Json.JsonSerializer.Serialize(input)}");
                
                var task = new ProjectTask
                {
                    Title = input.Title,
                    Description = input.Description,
                    Priority = input.Priority,
                    Status = TaskStatus.Todo,
                    ProjectId = input.ProjectId,
                    AssignedToUserId = input.AssignedToUserId,
                    AssignedByUserId = AbpSession.GetUserId(),
                    DueDate = input.DueDate,
                    Location = input.Location,
                    Notes = input.Notes
                };

                Console.WriteLine($"📝 Task object created: {task.Title}, ProjectId: {task.ProjectId}, AssignedToUserId: {task.AssignedToUserId}");
                Console.WriteLine($"👤 Current user ID: {AbpSession.GetUserId()}");

                await _taskRepository.InsertAsync(task);
                Console.WriteLine("💾 Task inserted to repository");
                
                await CurrentUnitOfWork.SaveChangesAsync();
                Console.WriteLine("💾 Unit of work saved");

                var result = await GetAsync(task.Id);
                Console.WriteLine($"✅ Task created successfully with ID: {result.Id}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR in TaskAppService.CreateAsync: {ex.Message}");
                Console.WriteLine($"💥 Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<TaskDto> UpdateAsync(UpdateTaskDto input)
        {
            var task = await _taskRepository.GetAsync(input.Id);

            task.Title = input.Title;
            task.Description = input.Description;
            task.Status = input.Status;
            task.Priority = input.Priority;
            task.AssignedToUserId = input.AssignedToUserId;
            task.DueDate = input.DueDate;
            task.Location = input.Location;
            task.Notes = input.Notes;

            if (input.Status == Entities.TaskStatus.Completed && task.CompletedDate == null)
            {
                task.CompletedDate = DateTime.Now;
            }

            await _taskRepository.UpdateAsync(task);
            return await GetAsync(task.Id);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _taskRepository.DeleteAsync(id);
        }

        public async Task<TaskDto> UpdateStatusAsync(Guid taskId, UpdateTaskStatusDto input)
        {
            var task = await _taskRepository.GetAsync(taskId);
            task.Status = input.Status;

            if (input.Status == TaskStatus.Completed && task.CompletedDate == null)
            {
                task.CompletedDate = DateTime.Now;
            }

            await _taskRepository.UpdateAsync(task);

            // Add comment if provided
            if (!string.IsNullOrEmpty(input.Comment))
            {
                var comment = new TaskComment
                {
                    TaskId = taskId,
                    UserId = AbpSession.GetUserId(),
                    Comment = input.Comment
                };
                await _taskCommentRepository.InsertAsync(comment);
            }

            return await GetAsync(taskId);
        }

        public async Task<TaskDto> CompleteTaskAsync(Guid taskId, string comment = null)
        {
            return await UpdateStatusAsync(taskId, new UpdateTaskStatusDto 
            { 
                Status = Entities.TaskStatus.Completed, 
                Comment = comment 
            });
        }

        public async Task<TaskDto> AssignTaskAsync(Guid taskId, long userId)
        {
            var task = await _taskRepository.GetAsync(taskId);
            task.AssignedToUserId = userId;
            await _taskRepository.UpdateAsync(task);

            return await GetAsync(taskId);
        }

        public async Task<List<TaskSummaryDto>> GetUserTasksAsync(long userId)
        {
            var tasks = await _taskRepository.GetAll()
                .Include(x => x.Project)
                .Include(x => x.AssignedToUser)
                .Include(x => x.Comments)
                .Include(x => x.Photos)
                .Where(x => x.AssignedToUserId == userId)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();

            return tasks.Select(task => new TaskSummaryDto
            {
                Id = task.Id,
                Title = task.Title,
                Status = task.Status,
                Priority = task.Priority,
                ProjectName = task.Project?.Name,
                AssignedToUserName = task.AssignedToUser != null ? $"{task.AssignedToUser.FirstName} {task.AssignedToUser.LastName}" : null,
                DueDate = task.DueDate,
                CreationTime = task.CreationTime,
                PhotoCount = task.Photos?.Count ?? 0,
                CommentCount = task.Comments?.Count ?? 0
            }).ToList();
        }

        public async Task<List<TaskSummaryDto>> GetProjectTasksAsync(Guid projectId)
        {
            var tasks = await _taskRepository.GetAll()
                .Include(x => x.Project)
                .Include(x => x.AssignedToUser)
                .Include(x => x.Comments)
                .Include(x => x.Photos)
                .Where(x => x.ProjectId == projectId)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();

            return tasks.Select(task => new TaskSummaryDto
            {
                Id = task.Id,
                Title = task.Title,
                Status = task.Status,
                Priority = task.Priority,
                ProjectName = task.Project?.Name,
                AssignedToUserName = task.AssignedToUser != null ? $"{task.AssignedToUser.FirstName} {task.AssignedToUser.LastName}" : null,
                DueDate = task.DueDate,
                CreationTime = task.CreationTime,
                PhotoCount = task.Photos?.Count ?? 0,
                CommentCount = task.Comments?.Count ?? 0
            }).ToList();
        }

        public async Task<List<TaskSummaryDto>> GetTasksByStatusAsync(TaskStatus status)
        {
            var tasks = await _taskRepository.GetAll()
                .Include(x => x.Project)
                .Include(x => x.AssignedToUser)
                .Include(x => x.Comments)
                .Include(x => x.Photos)
                .Where(x => x.Status == status)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();

            return tasks.Select(task => new TaskSummaryDto
            {
                Id = task.Id,
                Title = task.Title,
                Status = task.Status,
                Priority = task.Priority,
                ProjectName = task.Project?.Name,
                AssignedToUserName = task.AssignedToUser != null ? $"{task.AssignedToUser.FirstName} {task.AssignedToUser.LastName}" : null,
                DueDate = task.DueDate,
                CreationTime = task.CreationTime,
                PhotoCount = task.Photos?.Count ?? 0,
                CommentCount = task.Comments?.Count ?? 0
            }).ToList();
        }

        public async Task<List<TaskSummaryDto>> GetOverdueTasksAsync()
        {
            var now = DateTime.Now;
            var tasks = await _taskRepository.GetAll()
                .Include(x => x.Project)
                .Include(x => x.AssignedToUser)
                .Include(x => x.Comments)
                .Include(x => x.Photos)
                .Where(x => x.DueDate.HasValue && x.DueDate.Value < now && x.Status != Entities.TaskStatus.Completed)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();

            return tasks.Select(task => new TaskSummaryDto
            {
                Id = task.Id,
                Title = task.Title,
                Status = task.Status,
                Priority = task.Priority,
                ProjectName = task.Project?.Name,
                AssignedToUserName = task.AssignedToUser != null ? $"{task.AssignedToUser.FirstName} {task.AssignedToUser.LastName}" : null,
                DueDate = task.DueDate,
                CreationTime = task.CreationTime,
                PhotoCount = task.Photos?.Count ?? 0,
                CommentCount = task.Comments?.Count ?? 0
            }).ToList();
        }

        public async Task<List<TaskSummaryDto>> GetRecentTasksAsync(int count = 10)
        {
            var tasks = await _taskRepository.GetAll()
                .Include(x => x.Project)
                .Include(x => x.AssignedToUser)
                .Include(x => x.Comments)
                .Include(x => x.Photos)
                .OrderByDescending(x => x.CreationTime)
                .Take(count)
                .ToListAsync();

            return tasks.Select(task => new TaskSummaryDto
            {
                Id = task.Id,
                Title = task.Title,
                Status = task.Status,
                Priority = task.Priority,
                ProjectName = task.Project?.Name,
                AssignedToUserName = task.AssignedToUser != null ? $"{task.AssignedToUser.FirstName} {task.AssignedToUser.LastName}" : null,
                DueDate = task.DueDate,
                CreationTime = task.CreationTime,
                PhotoCount = task.Photos?.Count ?? 0,
                CommentCount = task.Comments?.Count ?? 0
            }).ToList();
        }

        public async Task<TaskCommentDto> AddCommentAsync(CreateTaskCommentDto input)
        {
            var comment = new TaskComment
            {
                TaskId = input.TaskId,
                UserId = AbpSession.GetUserId(),
                Comment = input.Comment
            };

            await _taskCommentRepository.InsertAsync(comment);
            await CurrentUnitOfWork.SaveChangesAsync();

            var savedComment = await _taskCommentRepository.GetAll()
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == comment.Id);

            var commentDto = ObjectMapper.Map<TaskCommentDto>(savedComment);
            if (savedComment.User != null)
                commentDto.UserName = $"{savedComment.User.FirstName} {savedComment.User.LastName}";

            return commentDto;
        }

        public async Task<List<TaskCommentDto>> GetTaskCommentsAsync(Guid taskId)
        {
            var comments = await _taskCommentRepository.GetAll()
                .Include(x => x.User)
                .Where(x => x.TaskId == taskId)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();

            return comments.Select(comment => 
            {
                var commentDto = ObjectMapper.Map<TaskCommentDto>(comment);
                if (comment.User != null)
                    commentDto.UserName = $"{comment.User.FirstName} {comment.User.LastName}";
                return commentDto;
            }).ToList();
        }

        public async Task DeleteCommentAsync(Guid commentId)
        {
            await _taskCommentRepository.DeleteAsync(commentId);
        }

        public async Task<TaskPhotoDto> AddPhotoAsync(CreateTaskPhotoDto input)
        {
            var photo = new TaskPhoto
            {
                TaskId = input.TaskId,
                UserId = AbpSession.GetUserId(),
                FilePath = input.FilePath,
                FileName = input.FileName,
                Description = input.Description,
                FileSize = input.FileSize,
                ContentType = input.ContentType
            };

            await _taskPhotoRepository.InsertAsync(photo);
            await CurrentUnitOfWork.SaveChangesAsync();

            var savedPhoto = await _taskPhotoRepository.GetAll()
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == photo.Id);

            var photoDto = ObjectMapper.Map<TaskPhotoDto>(savedPhoto);
            if (savedPhoto.User != null)
                photoDto.UserName = $"{savedPhoto.User.FirstName} {savedPhoto.User.LastName}";

            return photoDto;
        }

        public async Task<List<TaskPhotoDto>> GetTaskPhotosAsync(Guid taskId)
        {
            var photos = await _taskPhotoRepository.GetAll()
                .Include(x => x.User)
                .Where(x => x.TaskId == taskId)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();

            return photos.Select(photo => 
            {
                var photoDto = ObjectMapper.Map<TaskPhotoDto>(photo);
                if (photo.User != null)
                    photoDto.UserName = $"{photo.User.FirstName} {photo.User.LastName}";
                return photoDto;
            }).ToList();
        }

        public async Task DeletePhotoAsync(Guid photoId)
        {
            await _taskPhotoRepository.DeleteAsync(photoId);
        }

        public async Task<TaskStatsDto> GetTaskStatsAsync(Guid? projectId = null, long? userId = null)
        {
            var query = _taskRepository.GetAll()
                .WhereIf(projectId.HasValue, x => x.ProjectId == projectId.Value)
                .WhereIf(userId.HasValue, x => x.AssignedToUserId == userId.Value);

            var totalTasks = await query.CountAsync();
            var todoTasks = await query.CountAsync(x => x.Status == Entities.TaskStatus.Todo);
            var inProgressTasks = await query.CountAsync(x => x.Status == Entities.TaskStatus.InProgress);
            var completedTasks = await query.CountAsync(x => x.Status == Entities.TaskStatus.Completed);
            var overdueTasks = await query.CountAsync(x => x.DueDate.HasValue && x.DueDate.Value < DateTime.Now && x.Status != TaskStatus.Completed);
            var highPriorityTasks = await query.CountAsync(x => x.Priority == TaskPriority.High || x.Priority == TaskPriority.Critical);

            var recentTasks = await GetRecentTasksAsync(5);

            return new TaskStatsDto
            {
                TotalTasks = totalTasks,
                TodoTasks = todoTasks,
                InProgressTasks = inProgressTasks,
                CompletedTasks = completedTasks,
                OverdueTasks = overdueTasks,
                HighPriorityTasks = highPriorityTasks,
                CompletionRate = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0,
                RecentTasks = recentTasks
            };
        }

        public async Task<TaskStatsDto> GetCompanyTaskStatsAsync(Guid? companyId = null)
        {
            var query = _taskRepository.GetAll()
                .Include(x => x.Project)
                .WhereIf(companyId.HasValue, x => x.Project.CompanyId == companyId.Value);

            var totalTasks = await query.CountAsync();
            var todoTasks = await query.CountAsync(x => x.Status == Entities.TaskStatus.Todo);
            var inProgressTasks = await query.CountAsync(x => x.Status == Entities.TaskStatus.InProgress);
            var completedTasks = await query.CountAsync(x => x.Status == Entities.TaskStatus.Completed);
            var overdueTasks = await query.CountAsync(x => x.DueDate.HasValue && x.DueDate.Value < DateTime.Now && x.Status != Entities.TaskStatus.Completed);
            var highPriorityTasks = await query.CountAsync(x => x.Priority == TaskPriority.High || x.Priority == TaskPriority.Critical);

            var recentTasks = await GetRecentTasksAsync(5);

            return new TaskStatsDto
            {
                TotalTasks = totalTasks,
                TodoTasks = todoTasks,
                InProgressTasks = inProgressTasks,
                CompletedTasks = completedTasks,
                OverdueTasks = overdueTasks,
                HighPriorityTasks = highPriorityTasks,
                CompletionRate = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0,
                RecentTasks = recentTasks
            };
        }

        public async Task<PagedResultDto<TaskDto>> SearchTasksAsync(string searchTerm, PagedAndSortedResultRequestDto input)
        {
            var searchInput = new PagedTaskRequestDto
            {
                SearchTerm = searchTerm,
                SkipCount = input.SkipCount,
                MaxResultCount = input.MaxResultCount,
                Sorting = input.Sorting
            };

            return await GetAllAsync(searchInput);
        }

        public async Task<List<TaskSummaryDto>> GetTasksByPriorityAsync(TaskPriority priority)
        {
            var tasks = await _taskRepository.GetAll()
                .Include(x => x.Project)
                .Include(x => x.AssignedToUser)
                .Include(x => x.Comments)
                .Include(x => x.Photos)
                .Where(x => x.Priority == priority)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();

            return tasks.Select(task => new TaskSummaryDto
            {
                Id = task.Id,
                Title = task.Title,
                Status = task.Status,
                Priority = task.Priority,
                ProjectName = task.Project?.Name,
                AssignedToUserName = task.AssignedToUser != null ? $"{task.AssignedToUser.FirstName} {task.AssignedToUser.LastName}" : null,
                DueDate = task.DueDate,
                CreationTime = task.CreationTime,
                PhotoCount = task.Photos?.Count ?? 0,
                CommentCount = task.Comments?.Count ?? 0
            }).ToList();
        }

        private async Task CreateSampleTasks()
        {
            try
            {
                // Önce mevcut kullanıcıları ve projeleri al
                var users = await _userRepository.GetAll().Take(5).ToListAsync();
                var projects = await _projectRepository.GetAll().Take(3).ToListAsync();

                if (!users.Any() || !projects.Any())
                {
                    // Eğer kullanıcı veya proje yoksa, basit test verileri oluştur
                    return;
                }

                var sampleTasks = new List<ProjectTask>
                {
                    new ProjectTask
                    {
                        Title = "Beton Kalite Kontrolü",
                        Description = "Acil Yapı Sitesi A Blok 3. kat beton dökümü kalite kontrolü yapılacak.",
                        Status = TaskStatus.Todo,
                        Priority = TaskPriority.High,
                        ProjectId = projects.First().Id,
                        AssignedToUserId = users.First().Id,
                        AssignedByUserId = users.First().Id,
                        DueDate = DateTime.Now.AddDays(2),
                        Location = "Acil Yapı Sitesi A Blok - 3. Kat",
                        Notes = "Beton karışım raporu eklenecek"
                    },
                    new ProjectTask
                    {
                        Title = "Elektrik Tesisatı Kontrolü",
                        Description = "Ofis Kompleksi B Projesi elektrik tesisatı güvenlik kontrolü.",
                        Status = TaskStatus.InProgress,
                        Priority = TaskPriority.Medium,
                        ProjectId = projects.First().Id,
                        AssignedToUserId = users.First().Id,
                        AssignedByUserId = users.First().Id,
                        DueDate = DateTime.Now.AddDays(5),
                        Location = "Ofis Kompleksi B - Zemin Kat",
                        Notes = "İlk kontroller tamamlandı, bazı kablolarda sorun var"
                    },
                    new ProjectTask
                    {
                        Title = "Çelik Konstrüksiyon Montajı",
                        Description = "Depo Projesi çelik konstrüksiyon montaj işlemleri.",
                        Status = TaskStatus.Completed,
                        Priority = TaskPriority.High,
                        ProjectId = projects.First().Id,
                        AssignedToUserId = users.First().Id,
                        AssignedByUserId = users.First().Id,
                        DueDate = DateTime.Now.AddDays(-1),
                        Location = "Depo Projesi - Ana Bina",
                        Notes = "Montaj işlemleri başarıyla tamamlandı"
                    },
                    new ProjectTask
                    {
                        Title = "Su Tesisatı Kontrolü",
                        Description = "Rezidans Projesi su tesisatı kontrol ve test işlemleri.",
                        Status = TaskStatus.Todo,
                        Priority = TaskPriority.Low,
                        ProjectId = projects.First().Id,
                        AssignedToUserId = users.First().Id,
                        AssignedByUserId = users.First().Id,
                        DueDate = DateTime.Now.AddDays(7),
                        Location = "Rezidans Projesi - 2. Kat",
                        Notes = "Su basınç testleri yapılacak"
                    },
                    new ProjectTask
                    {
                        Title = "İnşaat Demiri Kontrolü",
                        Description = "Villa Projesi inşaat demiri kalite ve miktar kontrolü.",
                        Status = TaskStatus.InProgress,
                        Priority = TaskPriority.Medium,
                        ProjectId = projects.First().Id,
                        AssignedToUserId = users.First().Id,
                        AssignedByUserId = users.First().Id,
                        DueDate = DateTime.Now.AddDays(3),
                        Location = "Villa Projesi - Temel Kat",
                        Notes = "Demir çapı ve aderans kontrolü yapılacak"
                    }
                };

                foreach (var task in sampleTasks)
                {
                    await _taskRepository.InsertAsync(task);
                }

                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Hata durumunda loglama yapılabilir
                Console.WriteLine($"Sample tasks oluşturulurken hata: {ex.Message}");
            }
        }
    }
}
