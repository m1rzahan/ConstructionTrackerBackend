using Abp.Application.Services;
using Abp.Application.Services.Dto;
using ConstructionTracker.Roles.Dto;
using ConstructionTracker.Users.Dto;
using System.Threading.Tasks;

namespace ConstructionTracker.Users;

public interface IUserAppService : IAsyncCrudAppService<UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>
{
    Task DeActivate(EntityDto<long> user);
    Task Activate(EntityDto<long> user);
    Task<ListResultDto<RoleDto>> GetRoles();
    Task ChangeLanguage(ChangeUserLanguageDto input);

    Task<bool> ChangePassword(ChangePasswordDto input);
}
