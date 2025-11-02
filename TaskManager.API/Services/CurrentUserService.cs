namespace TaskManager.API.Services;

using System.Security.Claims;
using TaskManager.Application.Common.Interfaces;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User
        .FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;

    public string? Role => _httpContextAccessor.HttpContext?.User
        .FindFirst("role")?.Value
        ?? _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin => string.Equals(Role, "admin", StringComparison.OrdinalIgnoreCase);
}
