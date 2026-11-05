// <copyright file="UpdateContactCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Contacts.Commands;

/// <summary>
/// Handler for <see cref="UpdateContactCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UpdateContactCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class UpdateContactCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<UpdateContactCommand, ContactDto?>
{
    /// <inheritdoc/>
    public Task<ContactDto?> Handle(
        UpdateContactCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<ContactDto?> HandleInternalAsync(
        UpdateContactCommand request,
        CancellationToken cancellationToken)
    {
        var contact = await unitOfWork
            .Repository<Contact>()
            .GetByIdAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (contact is null)
        {
            return null;
        }

        contact.Type = request.Type;
        contact.Country = request.Country;
        contact.CompanyName = request.CompanyName;
        contact.FirstName = request.FirstName;
        contact.LastName = request.LastName;
        contact.Email = request.Email;
        contact.Phone = request.Phone;
        contact.VatNumber = request.VatNumber;
        contact.FiscalCode = request.FiscalCode;
        contact.UidNumber = request.UidNumber;
        contact.SdiCode = request.SdiCode;
        contact.Pec = request.Pec;
        contact.Iban = request.Iban;
        contact.QrIban = request.QrIban;
        contact.PaymentTermsDays = request.PaymentTermsDays;
        contact.UpdatedAt = DateTime.UtcNow;

        unitOfWork
            .Repository<Contact>()
            .Update(contact);

        _ = await unitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return mapper.Map<ContactDto>(contact);
    }
}
