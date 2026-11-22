// <copyright file="TemplateParameter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// A parameter a template asks the user for.
/// </summary>
/// <param name="Key">The parameter key (lower case, e.g. "revenue_account").</param>
/// <param name="Type">The parameter type: account, amount or rate.</param>
public sealed record TemplateParameter(string Key, string Type);
