// <copyright file="ErnestHemingwayStyleExtension.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using DevExpress.AIIntegration.Extensions;

namespace FatturaFacile.Modules.CodeExamples; 
public class ErnestHemingwayStyleExtension : ProofreadExtension {
    public ErnestHemingwayStyleExtension(IServiceProvider serviceProvider)
        : base(serviceProvider) { }
    protected override string GetSystemPrompt(ProofreadRequest request) {
        return "Rewrite this text in the Ernest Hemingway style.";
    }
}
