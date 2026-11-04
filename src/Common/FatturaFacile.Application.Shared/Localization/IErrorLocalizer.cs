// <copyright file="IErrorLocalizer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

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
    /// <param name="errorCode">The semantic error code to localize.</param>
    /// <param name="culture">The target culture for localization.</param>
    /// <param name="arguments">Optional arguments for formatting the localized message.</param>
    /// <returns>The localized error message.</returns>
    public string Localize(string errorCode, string? culture = null, params object[] arguments);

    /// <summary>
    /// Localizes a domain error definition instance.
    /// </summary>
    /// <param name="error">The domain error definition to localize.</param>
    /// <param name="culture">The target culture for localization.</param>
    /// <returns>The localized error message.</returns>
    public string Localize(DomainErrorDefinition error, string? culture = null);
}
