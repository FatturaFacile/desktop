// <copyright file="InvoiceRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Ports;
using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core repository implementation for managing invoices, line items, and sequential numbering sequences.
/// </summary>
public class InvoiceRepository : RepositoryBase<Invoice>, IInvoiceRepository
{
    private readonly ICurrentTenantService _currentTenantService;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="currentTenantService">The ambient tenant context service.</param>
    public InvoiceRepository(AppDbContext context, ICurrentTenantService currentTenantService) : base(context)
    {
        _currentTenantService = currentTenantService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Invoice>> GetInvoicesWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(i => i.Contact)
            .Include(i => i.Items)
            .OrderByDescending(i => i.Date)
            .ThenByDescending(i => i.SequentialNumber)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Invoice?> GetInvoiceWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(i => i.Contact)
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(string Number, int SequentialNumber)> GetNextInvoiceNumberAsync(int fiscalYear, CancellationToken cancellationToken = default)
    {
        var tenantId = _currentTenantService.TenantId;
        if (!tenantId.HasValue)
        {
            throw new InvalidOperationException("Active tenant context is required to generate sequential invoice numbers.");
        }

        // Sequence lookup with ignore filters to find sequence for tenant
        var sequence = await Context.DocumentSequences
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(ds => ds.TenantId == tenantId.Value && ds.DocumentType == "invoice" && ds.FiscalYear == fiscalYear, cancellationToken);

        if (sequence == null)
        {
            sequence = new DocumentSequence
            {
                TenantId = tenantId.Value,
                DocumentType = "invoice",
                FiscalYear = fiscalYear,
                Prefix = "INV-",
                LastNumber = 0,
                Padding = 4,
            };
            _ = Context.DocumentSequences.Add(sequence);
        }

        sequence.LastNumber++;
        _ = await Context.SaveChangesAsync(cancellationToken);

        var formattedNumber = $"{sequence.Prefix}{fiscalYear}-{sequence.LastNumber.ToString().PadLeft(sequence.Padding, '0')}";
        return (formattedNumber, sequence.LastNumber);
    }
}
