// <copyright file="ContactRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Crm.Domain.Aggregates;
using FatturaFacile.Crm.Domain.Repositories;
using FatturaFacile.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Crm.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of the <see cref="IContactRepository"/> domain repository.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ContactRepository"/> class.
/// </remarks>
/// <param name="context">The CRM database context.</param>
public sealed class ContactRepository(CrmDbContext context) : IContactRepository
{
    private readonly CrmDbContext _context = context;

    /// <inheritdoc />
    public async Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(Contact contact, CancellationToken cancellationToken = default)
    {
        _ = await _context.Contacts.AddAsync(contact, cancellationToken);
        _ = await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Contact contact, CancellationToken cancellationToken = default)
    {
        _ = _context.Contacts.Update(contact);
        _ = await _context.SaveChangesAsync(cancellationToken);
    }
}
