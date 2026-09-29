using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;

namespace AgroVerde.Application.Services;

public interface IDashboardService
{
    Task<AppResponse<DashboardResponse>> ObterDashboardAsync(
        int propriedadeId);
}