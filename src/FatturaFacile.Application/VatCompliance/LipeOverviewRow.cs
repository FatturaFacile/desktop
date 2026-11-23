// <copyright file="LipeOverviewRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// An Italian VAT liquidation with the LIPE communication prepared for it.
/// </summary>
/// <param name="Return">The liquidation.</param>
/// <param name="Submission">The communication, or null when none was prepared.</param>
public sealed record LipeOverviewRow(VatReturn Return, LipeSubmission? Submission);
