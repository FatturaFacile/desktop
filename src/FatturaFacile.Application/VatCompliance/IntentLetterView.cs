// <copyright file="IntentLetterView.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// A declaration of intent with its customer name, erosion and residual plafond.
/// </summary>
/// <param name="Letter">The letter.</param>
/// <param name="ContactName">The name of the customer or supplier.</param>
/// <param name="Consumed">The eroded plafond.</param>
/// <param name="Residual">The plafond left.</param>
public sealed record IntentLetterView(IntentLetter Letter, string ContactName, decimal Consumed, decimal Residual);
