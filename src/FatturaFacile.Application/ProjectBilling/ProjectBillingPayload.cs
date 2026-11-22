// <copyright file="ProjectBillingPayload.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectBilling;

/// <summary>
/// The `data` payloads of one project billing pull, stored as received. An absent optional payload is the JSON text `null`.
/// </summary>
/// <param name="SummaryJson">The unbilled summary payload.</param>
/// <param name="ContractJson">The billing contract payload, or `null`.</param>
/// <param name="CertificatesJson">The SAL certificates payload (an array).</param>
/// <param name="RetentionJson">The retention balance payload, or `null`.</param>
/// <param name="RetainerJson">The retainer payload, or `null`.</param>
public sealed record ProjectBillingPayload(string SummaryJson, string ContractJson, string CertificatesJson, string RetentionJson, string RetainerJson);
