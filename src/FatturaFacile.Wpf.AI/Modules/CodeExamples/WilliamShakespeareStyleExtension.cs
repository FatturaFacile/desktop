// <copyright file="WilliamShakespeareStyleExtension.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using DevExpress.AIIntegration.Extensions;

namespace FatturaFacile.Modules.CodeExamples;

/// <summary>
/// AI proofreading extension that rewrites text into the poetic Elizabethan style of William Shakespeare.
/// </summary>
public class WilliamShakespeareStyleExtension : ProofreadExtension
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WilliamShakespeareStyleExtension"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider instance.</param>
    public WilliamShakespeareStyleExtension(
        IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <inheritdoc />
    protected override string GetSystemPrompt(
        ProofreadRequest request) =>
        "Rewrite this text in the William Shakespeare style.";
}
