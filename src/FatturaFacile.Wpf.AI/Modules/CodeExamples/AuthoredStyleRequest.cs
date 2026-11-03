// <copyright file="AuthoredStyleRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.AIIntegration.Extensions;

namespace FatturaFacile.Wpf.AI.Modules.CodeExamples;

/// <summary>
/// Text request specifying a target author whose style should be applied.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AuthoredStyleRequest"/> class.
/// </remarks>
/// <param name="author">The target author name.</param>
/// <param name="text">The source text to rewrite.</param>
public class AuthoredStyleRequest(
    string author,
    string text) : TextRequest(Text: text)
{

    /// <summary>
    /// Gets or sets the target author name.
    /// </summary>
    public string Author { get; set; } = author;

}
