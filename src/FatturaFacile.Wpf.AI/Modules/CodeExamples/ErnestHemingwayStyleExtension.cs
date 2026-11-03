// <copyright file="ErnestHemingwayStyleExtension.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.AIIntegration.Extensions;

namespace FatturaFacile.Wpf.AI.Modules.CodeExamples;

/// <summary>
/// AI proofreading extension that rewrites text into the direct, terse style of Ernest Hemingway.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ErnestHemingwayStyleExtension"/> class.
/// </remarks>
/// <param name="serviceProvider">The service provider instance.</param>
public class ErnestHemingwayStyleExtension(IServiceProvider serviceProvider) : ProofreadExtension(serviceProvider: serviceProvider)
{

    /// <inheritdoc />
    protected override string GetSystemPrompt(ProofreadRequest request) =>
        "Rewrite this text in the Ernest Hemingway style.";

}
