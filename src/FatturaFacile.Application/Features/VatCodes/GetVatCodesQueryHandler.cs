// <copyright file="GetVatCodesQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Ports;
using MediatR;

namespace FatturaFacile.Application.Features.VatCodes;

/// <summary>
/// Handles queries for active VAT codes and transforms them into DTO representations.
/// </summary>
public class GetVatCodesQueryHandler : 
    IRequestHandler<GetVatCodesQuery, IReadOnlyList<VatCodeDto>>,
    IQueryHandler<GetVatCodesQuery, IReadOnlyList<VatCodeDto>>
{
    private readonly IVatCodeRepository _vatCodeRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetVatCodesQueryHandler"/> class.
    /// </summary>
    /// <param name="vatCodeRepository">The repository used to access VAT code records.</param>
    public GetVatCodesQueryHandler(IVatCodeRepository vatCodeRepository)
    {
        _vatCodeRepository = vatCodeRepository;
    }

    /// <summary>
    /// Handles retrieval of active VAT codes via MediatR pipeline.
    /// </summary>
    /// <param name="query">The query request.</param>
    /// <param name="cancellationToken">Cancellation token to observe during processing.</param>
    /// <returns>A collection of <see cref="VatCodeDto"/> items.</returns>
    public async Task<IReadOnlyList<VatCodeDto>> Handle(GetVatCodesQuery query, CancellationToken cancellationToken)
    {
        var vatCodes = await _vatCodeRepository.GetActiveVatCodesAsync(cancellationToken);
        return vatCodes.Select(v => new VatCodeDto(
            v.Id,
            v.Code,
            v.Description,
            v.Rate,
            v.CountryIso,
            v.IsReverseCharge,
            v.IsSplitPayment)).ToList();
    }

    /// <summary>
    /// Handles retrieval of active VAT codes via application dispatcher.
    /// </summary>
    /// <param name="query">The query request.</param>
    /// <param name="cancellationToken">Cancellation token to observe during processing.</param>
    /// <returns>A collection of <see cref="VatCodeDto"/> items.</returns>
    public Task<IReadOnlyList<VatCodeDto>> HandleAsync(GetVatCodesQuery query, CancellationToken cancellationToken = default)
    {
        return Handle(query, cancellationToken);
    }
}
