// <copyright file="PlafondCheckResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// The outcome of the plafond check of an invoice without VAT under art. 8 c. 1 lett. c DPR 633/1972.
/// </summary>
/// <param name="Allowed">Whether the invoice may be issued.</param>
/// <param name="LetterId">The letter that covers the invoice, when there is one.</param>
/// <param name="ProtocolNumber">The telematic protocol of that letter.</param>
/// <param name="Required">The amount the invoice would erode.</param>
/// <param name="Residual">The plafond left on the letter before the invoice.</param>
/// <param name="BlockCode">The localization key of the reason when the invoice is blocked.</param>
/// <param name="NearExhaustion">Whether the invoice leaves less than a tenth of the plafond.</param>
public sealed record PlafondCheckResult(bool Allowed, Guid? LetterId, string? ProtocolNumber, decimal Required, decimal Residual, string? BlockCode, bool NearExhaustion);
