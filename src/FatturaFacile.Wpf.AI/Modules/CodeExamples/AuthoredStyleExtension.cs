// <copyright file="AuthoredStyleExtension.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.AIIntegration.Extensions;

namespace FatturaFacile.Wpf.AI.Modules.CodeExamples;

/// <summary>
/// AI text transformation extension that rewrites text in the literary style of a specified author.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AuthoredStyleExtension"/> class.
/// </remarks>
/// <param name="serviceProvider">The service provider instance.</param>
public class AuthoredStyleExtension(IServiceProvider serviceProvider) : ChangeTextExtension<AuthoredStyleRequest>(serviceProvider: serviceProvider)
{

    /// <inheritdoc />
    protected override string GetSystemPrompt(AuthoredStyleRequest request) =>
        $"Rewrite this text in the {request.Author} style";

}
