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
public sealed class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, ContactDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateContactCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="currentTenantService">The current tenant service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public CreateContactCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenantService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentTenantService = currentTenantService;
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public async Task<ContactDto> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Contact contact = new()
        {
            TenantId = _currentTenantService.TenantId,
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

        await _unitOfWork.Repository<Contact>().AddAsync(contact, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return _mapper.Map<ContactDto>(contact);
    }
}
