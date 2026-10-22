// <copyright file="AuthoredStyleRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.AIIntegration.Extensions;

namespace FatturaFacile.Wpf.AI.Modules.CodeExamples;

/// <summary>
/// Text request specifying a target author whose style should be applied.
/// </summary>
public class AuthoredStyleRequest : TextRequest
{

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthoredStyleRequest"/> class.
    /// </summary>
    /// <param name="author">The target author name.</param>
    /// <param name="text">The source text to rewrite.</param>
    public AuthoredStyleRequest(
        string author,
        string text) : base(Text: text)
    {
        Author = author;
    }

    /// <summary>
    /// Gets or sets the target author name.
    /// </summary>
    public string Author { get; set; }

}
