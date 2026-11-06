// <copyright file="UpdateContactCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Contacts.Commands;

/// <summary>
/// Command to update an existing contact.
/// </summary>
public sealed class UpdateContactCommand : IRequest<ContactDto?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;

    /// <summary>
    /// Gets or sets contact id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets relationship type.
    /// </summary>
    public string Type { get; set; } = "client";

    /// <summary>
    /// Gets or sets country code.
    /// </summary>
    public string Country { get; set; } = "IT";

    /// <summary>
    /// Gets or sets company name.
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Gets or sets first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets email.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets phone.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets VAT number.
    /// </summary>
    public string? VatNumber { get; set; }

    /// <summary>
    /// Gets or sets fiscal code.
    /// </summary>
    public string? FiscalCode { get; set; }

    /// <summary>
    /// Gets or sets Swiss UID.
    /// </summary>
    public string? UidNumber { get; set; }

    /// <summary>
    /// Gets or sets SDI code.
    /// </summary>
    public string? SdiCode { get; set; }

    /// <summary>
    /// Gets or sets PEC email.
    /// </summary>
    public string? Pec { get; set; }

    /// <summary>
    /// Gets or sets IBAN.
    /// </summary>
    public string? Iban { get; set; }

    /// <summary>
    /// Gets or sets Swiss QR-IBAN.
    /// </summary>
    public string? QrIban { get; set; }

    /// <summary>
    /// Gets or sets payment terms days.
    /// </summary>
    public int PaymentTermsDays { get; set; } = 30;
}
