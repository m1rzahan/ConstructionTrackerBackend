using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using ConstructionTracker.Entities;
using System;

namespace ConstructionTracker.ConstructionTracker.Dto
{
    [AutoMapFrom(typeof(ActivityLog))]
    public class ActivityLogDto : EntityDto<Guid>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public ActivityType ActivityType { get; set; }
        public long? UserId { get; set; }
        public string UserName { get; set; }
        public Guid? ProjectId { get; set; }
        public string ProjectName { get; set; }
        public Guid? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public DateTime ActivityDate { get; set; }
        public string Location { get; set; }
        public string AdditionalData { get; set; }
        public ActivityPriority Priority { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreationTime { get; set; }
        
        public string ActivityTypeDisplayName => GetActivityTypeDisplayName();
        public string PriorityDisplayName => GetPriorityDisplayName();
        public string TimeAgo => GetTimeAgo();

        private string GetActivityTypeDisplayName()
        {
            return ActivityType switch
            {
                ActivityType.UserLogin => "Kullanıcı Girişi",
                ActivityType.UserLogout => "Kullanıcı Çıkışı",
                ActivityType.ProjectCreated => "Proje Oluşturuldu",
                ActivityType.ProjectUpdated => "Proje Güncellendi",
                ActivityType.ProjectCompleted => "Proje Tamamlandı",
                ActivityType.QrCodeScanned => "QR Kod Tarandı",
                ActivityType.ReportGenerated => "Rapor Oluşturuldu",
                ActivityType.MaterialAdded => "Malzeme Eklendi",
                ActivityType.MaterialUsed => "Malzeme Kullanıldı",
                ActivityType.PersonnelAssigned => "Personel Atandı",
                ActivityType.PersonnelRemoved => "Personel Kaldırıldı",
                ActivityType.Other => "Diğer",
                _ => "Bilinmiyor"
            };
        }

        private string GetPriorityDisplayName()
        {
            return Priority switch
            {
                ActivityPriority.Low => "Düşük",
                ActivityPriority.Normal => "Normal",
                ActivityPriority.High => "Yüksek",
                ActivityPriority.Critical => "Kritik",
                _ => "Bilinmiyor"
            };
        }

        private string GetTimeAgo()
        {
            var timeSpan = DateTime.Now - ActivityDate;
            
            if (timeSpan.TotalMinutes < 1)
                return "Az önce";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} dakika önce";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} saat önce";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} gün önce";
            
            return ActivityDate.ToString("dd.MM.yyyy");
        }
    }

    public class CreateActivityLogDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public ActivityType ActivityType { get; set; }
        public long? UserId { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? CompanyId { get; set; }
        public DateTime ActivityDate { get; set; } = DateTime.Now;
        public string Location { get; set; }
        public string AdditionalData { get; set; }
        public ActivityPriority Priority { get; set; } = ActivityPriority.Normal;
    }

    public class DashboardActivityDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string TimeAgo { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
    }
} 