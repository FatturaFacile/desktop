// <copyright file="AuthoredStyleExtension.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using DevExpress.AIIntegration.Extensions;

namespace FatturaFacile.Modules.CodeExamples; 
public class AuthoredStyleExtension : ChangeTextExtension<AuthoredStyleRequest> {
    public AuthoredStyleExtension(IServiceProvider serviceProvider) : base(serviceProvider) { }

    protected override string GetSystemPrompt(AuthoredStyleRequest request) =>
        $"Rewrite this text in the {request.Author} style";
}
