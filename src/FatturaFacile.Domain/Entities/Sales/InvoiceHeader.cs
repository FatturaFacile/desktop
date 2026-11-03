// <copyright file="InvoiceHeader.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Entities.Sales;

/// <summary>
/// Represents the header information of an invoice, including document number, document date, and due date.
/// </summary>
/// <param name="DocumentNumber">The unique identifier of the invoice document.</param>
/// <param name="DocumentDate">The date when the invoice was issued.</param>
/// <param name="DueDate">The date by which the invoice payment is due.</param>
public sealed record InvoiceHeader(
    string DocumentNumber,
    DateTime DocumentDate,
    DateTime DueDate);
