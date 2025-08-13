using ConstructionTracker.Authorization.Users;
using ConstructionTracker.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstructionTracker.EntityFrameworkCore.Seed.ConstructionTracker
{
    public class ConstructionTrackerDataBuilder
    {
        private readonly ConstructionTrackerDbContext _context;

        public ConstructionTrackerDataBuilder(ConstructionTrackerDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            Console.WriteLine("🌱 ConstructionTrackerDataBuilder.Create() başladı");
            
            try
            {
                CreateCompanies();
                CreateDemoUsers();
                CreateProjects();
                CreateActivityLogs();
                
                Console.WriteLine("✅ ConstructionTrackerDataBuilder.Create() tamamlandı");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ConstructionTrackerDataBuilder.Create() hatası: {ex.Message}");
                Console.WriteLine($"💥 Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private void CreateCompanies()
        {
            Console.WriteLine("🏢 CreateCompanies() başladı");
            
            if (_context.Companies.Any())
            {
                Console.WriteLine("🏢 Companies zaten mevcut, atlanıyor");
                return;
            }

            Console.WriteLine("🏢 Companies oluşturuluyor...");
            var companies = new[]
            {
                new Company
                {
                    Id = Guid.NewGuid(),
                    Name = "Acil Yapı İnşaat",
                    Description = "İnşaat ve yapı sektöründe faaliyet gösteren şirket",
                    Address = "İstanbul, Türkiye",
                    Phone = "+90 212 555 0001",
                    Email = "info@acilyapi.com",
                    TaxNumber = "1234567890",
                    IsActive = true
                },
                new Company
                {
                    Id = Guid.NewGuid(),
                    Name = "Güven İnşaat Ltd.",
                    Description = "Alt yüklenici firma",
                    Address = "Ankara, Türkiye",
                    Phone = "+90 312 555 0002",
                    Email = "info@guveninsaat.com",
                    TaxNumber = "0987654321",
                    IsActive = true
                }
            };

            _context.Companies.AddRange(companies);
            _context.SaveChanges();
            Console.WriteLine($"🏢 {companies.Length} company oluşturuldu ve kaydedildi");
        }

        private void CreateDemoUsers()
        {
            Console.WriteLine("👥 CreateDemoUsers() başladı");
            
            // Get default admin user
            var adminUser = _context.Users.FirstOrDefault(u => u.UserName == "admin");
            var company1 = _context.Companies.FirstOrDefault(c => c.Name == "Acil Yapı İnşaat");
            
            Console.WriteLine($"👥 Admin user bulundu: {adminUser != null}");
            Console.WriteLine($"👥 Company1 bulundu: {company1 != null}");

            // Update admin user with our fields
            if (adminUser != null && company1 != null)
            {
                adminUser.FirstName = "Ahmet";
                adminUser.LastName = "Yılmaz";
                adminUser.Role = UserRoleType.Admin;
                adminUser.CompanyId = company1.Id;
                adminUser.EmailAddress = "admin@insaat.com";
                adminUser.SetNormalizedNames();
                _context.Users.Update(adminUser);
                _context.SaveChanges();
            }

            // Create additional demo users if they don't exist
            var company2 = _context.Companies.FirstOrDefault(c => c.Name == "Güven İnşaat Ltd.");
            
            var demoUsers = new[]
            {
                new { Email = "ofis@insaat.com", FirstName = "Ayşe", LastName = "Demir", Role = UserRoleType.OfficeStaff, Company = company1 },
                new { Email = "saha@insaat.com", FirstName = "Mehmet", LastName = "Kaya", Role = UserRoleType.SiteStaff, Company = company1 },
                new { Email = "yuklenci@insaat.com", FirstName = "Fatma", LastName = "Özkan", Role = UserRoleType.Subcontractor, Company = company2 }
            };

            foreach (var demoUser in demoUsers)
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.EmailAddress == demoUser.Email);
                if (existingUser == null && demoUser.Company != null)
                {
                    var newUser = new User
                    {
                        UserName = demoUser.Email,
                        EmailAddress = demoUser.Email,
                        Name = demoUser.FirstName,
                        Surname = demoUser.LastName,
                        FirstName = demoUser.FirstName,
                        LastName = demoUser.LastName,
                        Role = demoUser.Role,
                        CompanyId = demoUser.Company.Id,
                        IsActive = true,
                        IsEmailConfirmed = true,
                        Password = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==", // 123qwe
                        TenantId = 1
                    };
                    
                    newUser.SetNormalizedNames();
                    _context.Users.Add(newUser);
                }
                else if (existingUser != null && demoUser.Company != null)
                {
                    existingUser.FirstName = demoUser.FirstName;
                    existingUser.LastName = demoUser.LastName;
                    existingUser.Role = demoUser.Role;
                    existingUser.CompanyId = demoUser.Company.Id;
                    existingUser.SetNormalizedNames();
                    _context.Users.Update(existingUser);
                }
            }

            _context.SaveChanges();
            Console.WriteLine("👥 Demo users oluşturuldu ve kaydedildi");
        }

        private void CreateProjects()
        {
            if (_context.Projects.Any())
                return;

            var company1 = _context.Companies.FirstOrDefault(c => c.Name == "Acil Yapı İnşaat");
            if (company1 == null) return;

            var projects = new[]
            {
                new Project
                {
                    Id = Guid.NewGuid(),
                    Name = "Ataşehir Rezidans",
                    Description = "Lüks konut projesi",
                    CompanyId = company1.Id,
                    Address = "Ataşehir, İstanbul",
                    StartDate = DateTime.Now.AddDays(-60),
                    PlannedEndDate = DateTime.Now.AddDays(30),
                    Status = ProjectStatus.Active,
                    Progress = 75,
                    Budget = 5000000,
                    SpentAmount = 3750000,
                    IsActive = true
                },
                new Project
                {
                    Id = Guid.NewGuid(),
                    Name = "Pendik AVM",
                    Description = "Alışveriş merkezi projesi",
                    CompanyId = company1.Id,
                    Address = "Pendik, İstanbul",
                    StartDate = DateTime.Now.AddDays(-30),
                    PlannedEndDate = DateTime.Now.AddDays(90),
                    Status = ProjectStatus.Active,
                    Progress = 45,
                    Budget = 8000000,
                    SpentAmount = 3600000,
                    IsActive = true
                },
                new Project
                {
                    Id = Guid.NewGuid(),
                    Name = "Bakırköy Ofis",
                    Description = "Ofis kompleksi",
                    CompanyId = company1.Id,
                    Address = "Bakırköy, İstanbul",
                    StartDate = DateTime.Now.AddDays(-90),
                    PlannedEndDate = DateTime.Now.AddDays(-10),
                    Status = ProjectStatus.Delayed,
                    Progress = 90,
                    Budget = 3000000,
                    SpentAmount = 2800000,
                    IsActive = true
                }
            };

            _context.Projects.AddRange(projects);
            _context.SaveChanges();

            // Assign users to projects
            var users = _context.Users.Where(u => u.CompanyId == company1.Id).ToList();
            var projectList = _context.Projects.Where(p => p.CompanyId == company1.Id).ToList();

            foreach (var project in projectList)
            {
                foreach (var user in users)
                {
                    var userProject = new UserProject
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        ProjectId = project.Id,
                        Role = user.Role switch
                        {
                            UserRoleType.Admin => "Proje Yöneticisi",
                            UserRoleType.OfficeStaff => "Ofis Koordinatörü",
                            UserRoleType.SiteStaff => "Saha Sorumlusu",
                            _ => "Ekip Üyesi"
                        },
                        AssignedDate = DateTime.Now.AddDays(-30),
                        IsActive = true
                    };

                    _context.UserProjects.Add(userProject);
                }
            }

            _context.SaveChanges();
        }

        private void CreateActivityLogs()
        {
            if (_context.ActivityLogs.Any())
                return;

            var users = _context.Users.Where(u => !string.IsNullOrEmpty(u.FirstName)).ToList();
            var projects = _context.Projects.ToList();
            var company = _context.Companies.FirstOrDefault();

            if (users.Count < 2 || projects.Count < 1 || company == null)
                return;

            var activities = new List<ActivityLog>();

            // İlk kullanıcı için aktivite
            if (users.Count > 0)
            {
                activities.Add(new ActivityLog
                {
                    Id = Guid.NewGuid(),
                    Title = "Kullanıcı Girişi",
                    Description = $"{users[0].FirstName} {users[0].LastName} sisteme giriş yaptı",
                    ActivityType = ActivityType.UserLogin,
                    UserId = users[0].Id,
                    CompanyId = company.Id,
                    ActivityDate = DateTime.Now.AddMinutes(-5),
                    Priority = ActivityPriority.Normal
                });
            }

            // İkinci kullanıcı için aktivite
            if (users.Count > 1 && projects.Count > 0)
            {
                activities.Add(new ActivityLog
                {
                    Id = Guid.NewGuid(),
                    Title = "Yeni rapor eklendi",
                    Description = "Günlük ilerleme raporu oluşturuldu",
                    ActivityType = ActivityType.ReportGenerated,
                    UserId = users[1].Id,
                    ProjectId = projects[0].Id,
                    CompanyId = company.Id,
                    ActivityDate = DateTime.Now.AddHours(-1),
                    Priority = ActivityPriority.Normal
                });
            }

            // Üçüncü kullanıcı için aktivite
            if (users.Count > 2 && projects.Count > 1)
            {
                activities.Add(new ActivityLog
                {
                    Id = Guid.NewGuid(),
                    Title = "Malzeme eksikliği bildirimi",
                    Description = "Çimento stokları azaldı",
                    ActivityType = ActivityType.MaterialUsed,
                    UserId = users[2].Id,
                    ProjectId = projects[1].Id,
                    CompanyId = company.Id,
                    ActivityDate = DateTime.Now.AddHours(-3),
                    Priority = ActivityPriority.High
                });
            }

            if (activities.Any())
            {
                _context.ActivityLogs.AddRange(activities);
                _context.SaveChanges();
            }
        }
    }
} 