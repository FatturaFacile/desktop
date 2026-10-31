// <copyright file="GetVatCodesQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common;
using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.VatCodes;

/// <summary>
/// Query request to retrieve all active value-added tax (VAT) definitions and fiscal treatment codes.
/// </summary>
public record GetVatCodesQuery : 
    IRequest<IReadOnlyList<VatCodeDto>>, 
    IQuery<IReadOnlyList<VatCodeDto>>;
