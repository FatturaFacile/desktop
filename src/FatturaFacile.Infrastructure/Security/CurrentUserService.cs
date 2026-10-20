using FatturaFacile.Application.Common.Interfaces;

namespace FatturaFacile.Infrastructure.Security;

public sealed class CurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; private set; }
    public string? Username { get; private set; }
    public string? FullName { get; private set; }
    public string? Role { get; private set; }
    public bool IsAuthenticated => UserId.HasValue;

    public void SetUser(Guid userId, string username, string fullName, string role)
    {
        UserId = userId;
        Username = username;
        FullName = fullName;
        Role = role;
    }

    public void Clear()
    {
        UserId = null;
        Username = null;
        FullName = null;
        Role = null;
    }
}