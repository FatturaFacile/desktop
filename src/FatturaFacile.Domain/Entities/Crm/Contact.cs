using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Crm;

public sealed class Contact : Entity<Guid>
{
    public Guid ClientId { get; }
    public string FirstName { get; } = string.Empty;
    public string LastName { get; } = string.Empty;
    public string Email { get; } = string.Empty;
    public string? PhoneNumber { get; }
    public string? Role { get; }
    public bool IsPrimary { get; }

    internal Contact(Guid clientId, string firstName, string lastName, string email, string? phone, string? role, bool isPrimary)
    {
        Id = Guid.NewGuid();
        ClientId = clientId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phone;
        Role = role;
        IsPrimary = isPrimary;
    }

    private Contact()
    {
    }
}