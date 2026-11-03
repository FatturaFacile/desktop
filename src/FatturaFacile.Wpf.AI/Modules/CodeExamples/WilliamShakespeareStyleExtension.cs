// <copyright file="WilliamShakespeareStyleExtension.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.AIIntegration.Extensions;

namespace FatturaFacile.Wpf.AI.Modules.CodeExamples;

/// <summary>
/// AI proofreading extension that rewrites text into the poetic Elizabethan style of William Shakespeare.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WilliamShakespeareStyleExtension"/> class.
/// </remarks>
/// <param name="serviceProvider">The service provider instance.</param>
public class WilliamShakespeareStyleExtension(IServiceProvider serviceProvider) : ProofreadExtension(serviceProvider: serviceProvider)
{

    /// <inheritdoc />
    protected override string GetSystemPrompt(ProofreadRequest request) =>
        "Rewrite this text in the William Shakespeare style.";

}
