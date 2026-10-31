// <copyright file="CreateInvoiceCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Ports;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Services;
using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Handles invoice creation, coordinating sequential numbering generation, high-precision totals calculation,
/// line item persistence, and DTO projection within the current tenant workspace.
/// </summary>
public class CreateInvoiceCommandHandler : 
    IRequestHandler<CreateInvoiceCommand, InvoiceDto>,
    ICommandHandler<CreateInvoiceCommand, InvoiceDto>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IMapper _mapper;
    private readonly DocumentTotalsCalculator _totalsCalculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateInvoiceCommandHandler"/> class.
    /// </summary>
    /// <param name="invoiceRepository">The invoice repository dependency.</param>
    /// <param name="contactRepository">The contact repository dependency.</param>
    /// <param name="unitOfWork">The transactional unit of work.</param>
    /// <param name="currentTenantService">The ambient tenant context service.</param>
    /// <param name="mapper">The AutoMapper instance for DTO projection.</param>
    public CreateInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IContactRepository contactRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenantService,
        IMapper mapper)
    {
        _invoiceRepository = invoiceRepository;
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
        _currentTenantService = currentTenantService;
        _mapper = mapper;
        _totalsCalculator = new DocumentTotalsCalculator();
    }

    /// <summary>
    /// Executes the invoice creation workflow via MediatR pipeline.
    /// </summary>
    /// <param name="command">The invoice creation parameters.</param>
    /// <param name="cancellationToken">Cancellation token to observe during processing.</param>
    /// <returns>The newly created and persisted <see cref="InvoiceDto"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no active tenant is selected or the customer contact is invalid.</exception>
    public async Task<InvoiceDto> Handle(CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        if (_currentTenantService.TenantId == null)
        {
            throw new InvalidOperationException("No active tenant context selected.");
        }

        var tenantId = _currentTenantService.TenantId.Value;
        var contact = await _contactRepository.GetByIdAsync(command.ContactId, cancellationToken);
        if (contact == null)
        {
            throw new InvalidOperationException($"Contact '{command.ContactId}' not found.");
        }

        var fiscalYear = command.Date.Year;
        var (number, seqNumber) = await _invoiceRepository.GetNextInvoiceNumberAsync(fiscalYear, cancellationToken);

        var calcInputs = command.Items.Select(x => new DocumentLineInput(
            x.ProductId,
            x.Description,
            x.Quantity,
            x.UnitPrice,
            x.TaxRate,
            x.DiscountPercentage)).ToList();

        var calcResult = _totalsCalculator.Calculate(calcInputs, command.Currency);

        var invoice = new Invoice
        {
            TenantId = tenantId,
            ContactId = command.ContactId,
            InvoiceType = command.InvoiceType,
            InvoiceNumber = number,
            FiscalYear = fiscalYear,
            SequentialNumber = seqNumber,
            Date = command.Date,
            DueDate = command.DueDate,
            Currency = command.Currency.ToUpperInvariant(),
            ExchangeRate = 1.0m,
            TotalNet = calcResult.TotalNet,
            TotalTax = calcResult.TotalTax,
            TotalGross = calcResult.TotalGross,
            RoundingAmount = calcResult.RoundingAmount,
            PaymentStatus = PaymentStatus.Unpaid,
            Notes = command.Notes,
            BankName = command.BankName ?? _currentTenantService.CurrentTenant?.Name,
            BankIban = command.BankIban ?? _currentTenantService.CurrentTenant?.Iban,
            QrIban = command.QrIban ?? _currentTenantService.CurrentTenant?.QrIban,
        };

        foreach (var item in calcResult.Items)
        {
            invoice.Items.Add(new InvoiceItem
            {
                TenantId = tenantId,
                ProductId = item.ProductId,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TaxRate = item.TaxRate,
                DiscountPercentage = item.DiscountPercentage,
                LineTotal = item.LineTotal,
            });
        }

        await _invoiceRepository.AddAsync(invoice, cancellationToken);
        _ = await _unitOfWork.SaveChangesAsync(cancellationToken);

        var loaded = await _invoiceRepository.GetInvoiceWithDetailsAsync(invoice.Id, cancellationToken);
        return _mapper.Map<InvoiceDto>(loaded ?? invoice);
    }

    /// <summary>
    /// Executes the invoice creation workflow via application dispatcher.
    /// </summary>
    /// <param name="command">The invoice creation parameters.</param>
    /// <param name="cancellationToken">Cancellation token to observe during processing.</param>
    /// <returns>The newly created and persisted <see cref="InvoiceDto"/>.</returns>
    public Task<InvoiceDto> HandleAsync(CreateInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        return Handle(command, cancellationToken);
    }
}
