namespace FatturaFacile.Application.Shared.Context;

/// <summary>
/// Provides contextual information regarding the currently authenticated user, active tenant, and UI language culture.
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>
    /// Gets the unique identifier of the currently authenticated user, if any.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the unique identifier of the active tenant scope, if any.
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// Gets the user's selected culture code (e.g., 'en-US', 'it-CH', 'de-CH', 'fr-CH'), falling back to 'en-US'.
    /// </summary>
    string Culture { get; }
}

/// <summary>
/// Default ambient implementation of <see cref="ICurrentUserContext"/>.
/// </summary>
public class CurrentUserContext : ICurrentUserContext
{
    public Guid? UserId { get; set; }
    public Guid? TenantId { get; set; }
    public string Culture { get; set; } = "en-US";
}
