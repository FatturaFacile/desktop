// <copyright file="CopilotOptions.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Infrastructure.Copilot;

/// <summary>
/// Connection settings of the ERP Copilot service, read from the "Copilot" configuration section.
/// </summary>
public sealed class CopilotOptions
{
    private const char Slash = '/';

    /// <summary>
    /// Gets or sets the address of the web API root including the "/api" segment, e.g. "https://erp.example.com/api/".
    /// Empty when the Copilot is not configured (the cached history stays viewable).
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Bearer access token of the signed-in user.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets the parsed base address, or null when not configured or invalid.
    /// </summary>
    /// <returns>The address ending with a slash, or null.</returns>
    public Uri? GetBaseAddress()
    {
        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var address))
        {
            return null;
        }

        var text = address.AbsoluteUri;
        return text[^1] == Slash ? address : new Uri(text + Slash);
    }
}
