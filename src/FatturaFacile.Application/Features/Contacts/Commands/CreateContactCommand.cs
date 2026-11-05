// <copyright file="CreateContactCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Contacts.Commands;

/// <summary>
/// Command to create a new client or supplier contact.
/// </summary>
public sealed class CreateContactCommand : IRequest<ContactDto>
{
    /// <summary>
    /// Gets or sets relationship type ("client", "supplier", "both").
    /// </summary>
    public string Type { get; set; } = "client";

    /// <summary>
    /// Gets or sets country ISO code ("IT", "CH").
    /// </summary>
    public string Country { get; set; } = "IT";

    /// <summary>
    /// Gets or sets company or commercial trade name.
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
    /// Gets or sets VAT registration number.
    /// </summary>
    public string? VatNumber { get; set; }

    /// <summary>
    /// Gets or sets fiscal code.
    /// </summary>
    public string? FiscalCode { get; set; }

    /// <summary>
    /// Gets or sets Swiss UID number.
    /// </summary>
    public string? UidNumber { get; set; }

    /// <summary>
    /// Gets or sets Italian SDI code.
    /// </summary>
    public string? SdiCode { get; set; }

    /// <summary>
    /// Gets or sets PEC email.
    /// </summary>
    public string? Pec { get; set; }

    /// <summary>
    /// Gets or sets bank IBAN.
    /// </summary>
    public string? Iban { get; set; }

    /// <summary>
    /// Gets or sets Swiss QR-IBAN.
    /// </summary>
    public string? QrIban { get; set; }

    /// <summary>
    /// Gets or sets payment terms in days.
    /// </summary>
    public int PaymentTermsDays { get; set; } = 30;
}
