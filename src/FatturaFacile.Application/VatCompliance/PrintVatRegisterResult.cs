// <copyright file="PrintVatRegisterResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Models;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// The outcome of a definitive print of a VAT register.
/// </summary>
/// <param name="Run">The audit record of the print.</param>
/// <param name="Pdf">The printed register.</param>
public sealed record PrintVatRegisterResult(VatRegisterPrintRun Run, GeneratedFile Pdf);
