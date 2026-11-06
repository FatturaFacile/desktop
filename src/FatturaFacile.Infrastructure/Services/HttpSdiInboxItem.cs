// <copyright file="HttpSdiInboxItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Infrastructure.Services;

/// <summary>
/// A file returned by the inbox of the HTTP intermediary.
/// </summary>
public sealed class HttpSdiInboxItem
{
    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Base64 content.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
