// <copyright file="ProjectBillingRetention.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectBilling;

/// <summary>
/// The guarantee retention (ritenuta a garanzia, Garantierückbehalt) of a project.
/// </summary>
/// <param name="Withheld">The total withheld.</param>
/// <param name="Released">The total released.</param>
/// <param name="Balance">The retention still held.</param>
/// <param name="ReleaseRule">The contractual release rule.</param>
public sealed record ProjectBillingRetention(decimal Withheld, decimal Released, decimal Balance, string ReleaseRule);
