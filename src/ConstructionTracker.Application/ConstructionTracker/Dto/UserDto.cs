using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using ConstructionTracker.Authorization.Users;
using System;

namespace ConstructionTracker.ConstructionTracker.Dto
{
    [AutoMapFrom(typeof(User))]
    public class ConstructionTrackerUserDto : EntityDto<long>
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserRoleType Role { get; set; }
        public Guid? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class CreateUserDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserRoleType Role { get; set; }
        public Guid? CompanyId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateUserDto : EntityDto<long>
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserRoleType Role { get; set; }
        public Guid? CompanyId { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserLoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UserLoginResultDto
    {
        public ConstructionTrackerUserDto User { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
} 