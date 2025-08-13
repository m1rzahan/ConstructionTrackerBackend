using Abp.Authorization.Users;
using Abp.Extensions;
using ConstructionTracker.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ConstructionTracker.Authorization.Users;

public class User : AbpUser<User>
{
    public const string DefaultPassword = "123456"; // Android'deki demo şifre ile uyumlu

    // Android uygulamasındaki User modeline uygun ek alanlar
    [StringLength(100)]
    public string FirstName { get; set; }
    
    [StringLength(100)]
    public string LastName { get; set; }
    
    public UserRoleType Role { get; set; }
    
    public Guid? CompanyId { get; set; }
    
    public DateTime? LastLoginAt { get; set; }
    
    // Additional personnel fields
    [StringLength(500)]
    public string Address { get; set; }
    
    public DateTime? BirthDate { get; set; }
    
    [StringLength(200)]
    public string Position { get; set; }
    
    public decimal? Salary { get; set; }
    
    public DateTime? HireDate { get; set; }
    
    // Navigation properties
    public virtual Company Company { get; set; }
    public virtual ICollection<UserProject> UserProjects { get; set; }
    public virtual ICollection<ActivityLog> ActivityLogs { get; set; }
    public virtual ICollection<QrCodeScan> QrCodeScans { get; set; }

    public User()
    {
        UserProjects = new HashSet<UserProject>();
        ActivityLogs = new HashSet<ActivityLog>();
        QrCodeScans = new HashSet<QrCodeScan>();
    }

    public static string CreateRandomPassword()
    {
        return Guid.NewGuid().ToString("N").Truncate(16);
    }

    public static User CreateTenantAdminUser(int tenantId, string emailAddress)
    {
        var user = new User
        {
            TenantId = tenantId,
            UserName = AdminUserName,
            Name = AdminUserName,
            Surname = AdminUserName,
            FirstName = "Admin",
            LastName = "User",
            EmailAddress = emailAddress,
            Role = UserRoleType.Admin,
            Roles = new List<UserRole>()
        };

        user.SetNormalizedNames();

        return user;
    }
}

// Android uygulamasındaki UserRole enum'una uygun
public enum UserRoleType
{
    Admin = 1,
    OfficeStaff = 2,
    SiteStaff = 3,
    Subcontractor = 4
}
