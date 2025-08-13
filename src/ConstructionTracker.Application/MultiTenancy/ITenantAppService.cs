using Abp.Application.Services;
using ConstructionTracker.MultiTenancy.Dto;

namespace ConstructionTracker.MultiTenancy;

public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
{
}

