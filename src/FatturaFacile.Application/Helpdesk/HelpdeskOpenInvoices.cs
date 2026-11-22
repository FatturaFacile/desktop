// <copyright file="HelpdeskOpenInvoices.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// The open invoices of the customer.
/// </summary>
/// <param name="Count">The number of open invoices.</param>
/// <param name="OverdueCount">The number of overdue invoices.</param>
/// <param name="TotalGross">The gross total of the open invoices.</param>
public sealed record HelpdeskOpenInvoices(int Count, int OverdueCount, decimal TotalGross);
