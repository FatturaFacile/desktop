// <copyright file="CreateContactCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Contacts.Commands;

/// <summary>
/// Handler for <see cref="CreateContactCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreateContactCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentTenantService">The current tenant service.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class CreateContactCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenantService,
    IMapper mapper)
    : IRequestHandler<CreateContactCommand, ContactDto>
{
    /// <inheritdoc/>
    public Task<ContactDto> Handle(
        CreateContactCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(
            request,
            cancellationToken);
    }

    private async Task<ContactDto> HandleInternalAsync(
        CreateContactCommand request,
        CancellationToken cancellationToken)
    {
        Contact contact = new()
        {
            TenantId = currentTenantService.TenantId,
            Type = request.Type,
            Country = request.Country,
            CompanyName = request.CompanyName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            VatNumber = request.VatNumber,
            FiscalCode = request.FiscalCode,
            UidNumber = request.UidNumber,
            SdiCode = request.SdiCode,
            Pec = request.Pec,
            Iban = request.Iban,
            QrIban = request.QrIban,
            PaymentTermsDays = request.PaymentTermsDays,
        };

        await unitOfWork
            .Repository<Contact>()
            .AddAsync(contact, cancellationToken)
            .ConfigureAwait(false);

        _ = await unitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return mapper.Map<ContactDto>(contact);
    }
}
