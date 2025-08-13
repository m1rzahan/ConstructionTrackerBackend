using Abp.Zero.EntityFrameworkCore;
using ConstructionTracker.Authorization.Roles;
using ConstructionTracker.Authorization.Users;
using ConstructionTracker.MultiTenancy;
using ConstructionTracker.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstructionTracker.EntityFrameworkCore;

public class ConstructionTrackerDbContext : AbpZeroDbContext<Tenant, Role, User, ConstructionTrackerDbContext>
{
    /* Define a DbSet for each entity of the application */
    
    // Construction Tracker specific entities
    public DbSet<Company> Companies { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<UserProject> UserProjects { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<QrCodeScan> QrCodeScans { get; set; }
    public DbSet<ProjectMaterial> ProjectMaterials { get; set; }

    public ConstructionTrackerDbContext(DbContextOptions<ConstructionTrackerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure entity relationships and constraints
        
        // User - Company relationship
        builder.Entity<User>()
            .HasOne(u => u.Company)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);

        // Project - Company relationship
        builder.Entity<Project>()
            .HasOne(p => p.Company)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserProject relationships
        builder.Entity<UserProject>()
            .HasOne(up => up.User)
            .WithMany(u => u.UserProjects)
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserProject>()
            .HasOne(up => up.Project)
            .WithMany(p => p.UserProjects)
            .HasForeignKey(up => up.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // ActivityLog relationships
        builder.Entity<ActivityLog>()
            .HasOne(al => al.User)
            .WithMany(u => u.ActivityLogs)
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<ActivityLog>()
            .HasOne(al => al.Project)
            .WithMany(p => p.ActivityLogs)
            .HasForeignKey(al => al.ProjectId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<ActivityLog>()
            .HasOne(al => al.Company)
            .WithMany()
            .HasForeignKey(al => al.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);

        // QrCodeScan relationships
        builder.Entity<QrCodeScan>()
            .HasOne(qr => qr.User)
            .WithMany(u => u.QrCodeScans)
            .HasForeignKey(qr => qr.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<QrCodeScan>()
            .HasOne(qr => qr.Project)
            .WithMany(p => p.QrCodeScans)
            .HasForeignKey(qr => qr.ProjectId)
            .OnDelete(DeleteBehavior.NoAction);

        // ProjectMaterial relationships
        builder.Entity<ProjectMaterial>()
            .HasOne(pm => pm.Project)
            .WithMany(p => p.ProjectMaterials)
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for better performance
        builder.Entity<User>()
            .HasIndex(u => u.CompanyId);

        builder.Entity<Project>()
            .HasIndex(p => p.CompanyId);

        builder.Entity<UserProject>()
            .HasIndex(up => new { up.UserId, up.ProjectId })
            .IsUnique();

        builder.Entity<ActivityLog>()
            .HasIndex(al => al.ActivityDate);

        builder.Entity<QrCodeScan>()
            .HasIndex(qr => qr.ScanDate);
    }
}
