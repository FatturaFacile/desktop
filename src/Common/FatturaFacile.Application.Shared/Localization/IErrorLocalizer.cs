using FatturaFacile.Domain.Shared.Errors;

namespace FatturaFacile.Application.Shared.Localization;

/// <summary>
/// Service providing localized strings for semantic domain errors and validation codes.
/// </summary>
public interface IErrorLocalizer
{
    /// <summary>
    /// Localizes an error message given its semantic error code and target culture.
    /// </summary>
    string Localize(string errorCode, string? culture = null, params object[] arguments);

    /// <summary>
    /// Localizes a domain error definition instance.
    /// </summary>
    string Localize(DomainErrorDefinition error, string? culture = null);
}
