// <copyright file="UpdateTenantCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Validation;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Tenants.Commands;

/// <summary>
/// Handler for <see cref="UpdateTenantCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UpdateTenantCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentTenantService">The active tenant.</param>
/// <param name="mapper">The mapper.</param>
public sealed class UpdateTenantCommandHandler(IUnitOfWork unitOfWork, ICurrentTenantService currentTenantService, IMapper mapper)
    : IRequestHandler<UpdateTenantCommand, TenantDto>
{
    /// <inheritdoc/>
    public Task<TenantDto> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Tenant.Id != currentTenantService.TenantId
            ? throw new UnauthorizedAccessException("È possibile modificare solo l'azienda attiva.")
            : HandleInternalAsync(request.Tenant, cancellationToken);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task<TenantDto> HandleInternalAsync(TenantDto data, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<Tenant>();
        var tenant = await repository.GetByIdAsync(data.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Azienda non trovata.");

        tenant.Name = data.Name.Trim();
        tenant.LegalName = data.LegalName.Trim();
        tenant.FiscalCode = Clean(data.FiscalCode);
        tenant.VatNumber = Clean(data.VatNumber);
        tenant.UidNumber = Clean(data.UidNumber);
        tenant.Iban = Clean(data.Iban) is { } iban ? IbanValidator.Normalize(iban) : null;
        tenant.QrIban = Clean(data.QrIban) is { } qrIban ? IbanValidator.Normalize(qrIban) : null;
        tenant.SdiCode = Clean(data.SdiCode)?.ToUpperInvariant();
        tenant.PecEmail = Clean(data.PecEmail);
        tenant.Street = Clean(data.Street);
        tenant.HouseNumber = Clean(data.HouseNumber);
        tenant.PostalCode = Clean(data.PostalCode);
        tenant.City = Clean(data.City);
        tenant.Province = Clean(data.Province)?.ToUpperInvariant();
        tenant.TaxRegime = data.TaxRegime;

        repository.Update(tenant);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        currentTenantService.SetCurrentTenant(tenant.Id, tenant.Name, tenant.Country, tenant.DefaultCurrency);
        return mapper.Map<TenantDto>(tenant);
    }
}
