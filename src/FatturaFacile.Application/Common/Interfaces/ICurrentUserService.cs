namespace FatturaFacile.Application.Common.Interfaces;

public interface ICurrentUserService
{
    public Guid? UserId { get; }
    public string? Username { get; }
    public string? FullName { get; }
    public string? Role { get; }
    public bool IsAuthenticated { get; }
    public void SetUser(Guid userId, string username, string fullName, string role);
    public void Clear();
}
