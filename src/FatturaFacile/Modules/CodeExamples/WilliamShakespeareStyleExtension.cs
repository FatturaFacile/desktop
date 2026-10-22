// <copyright file="WilliamShakespeareStyleExtension.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using DevExpress.AIIntegration.Extensions;

namespace FatturaFacile.Modules.CodeExamples; 
public class WilliamShakespeareStyleExtension : ProofreadExtension {
    public WilliamShakespeareStyleExtension(IServiceProvider serviceProvider) : base(serviceProvider) { }

    protected override string GetSystemPrompt(ProofreadRequest request) =>
        "Rewrite this text in the William Shakespeare style.";
}
