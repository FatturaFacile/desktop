// <copyright file="AuthoredStyleRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.AIIntegration.Extensions;

namespace FatturaFacile.Modules.CodeExamples; 
public class AuthoredStyleRequest : TextRequest {
    public AuthoredStyleRequest(string author, string text) : base(text) {
        Author = author;
    }
    public string Author { get; set; }
}
