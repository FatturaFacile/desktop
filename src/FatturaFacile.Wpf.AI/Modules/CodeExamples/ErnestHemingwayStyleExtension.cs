// <copyright file="ErnestHemingwayStyleExtension.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using DevExpress.AIIntegration.Extensions;

namespace FatturaFacile.Modules.CodeExamples;

/// <summary>
/// AI proofreading extension that rewrites text into the direct, terse style of Ernest Hemingway.
/// </summary>
public class ErnestHemingwayStyleExtension : ProofreadExtension
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ErnestHemingwayStyleExtension"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider instance.</param>
    public ErnestHemingwayStyleExtension(
        IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    /// <inheritdoc />
    protected override string GetSystemPrompt(
        ProofreadRequest request) => "Rewrite this text in the Ernest Hemingway style.";
}
