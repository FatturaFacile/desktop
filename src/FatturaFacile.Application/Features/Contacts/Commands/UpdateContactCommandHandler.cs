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
public sealed class UpdateContactCommandHandler : IRequestHandler<UpdateContactCommand, ContactDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateContactCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public UpdateContactCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public async Task<ContactDto?> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var contact = await _unitOfWork.Repository<Contact>().GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (contact == null)
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

        _unitOfWork.Repository<Contact>().Update(contact);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return _mapper.Map<ContactDto>(contact);
    }
}
