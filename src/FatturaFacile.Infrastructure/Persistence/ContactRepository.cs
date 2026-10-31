// <copyright file="ContactRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Ports;
using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core repository implementation for client and customer contacts.
/// </summary>
public class ContactRepository : RepositoryBase<Contact>, IContactRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContactRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ContactRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Contact>> GetAllContactsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .OrderBy(c => c.CompanyName)
            .ThenBy(c => c.LastName)
            .ToListAsync(cancellationToken);
    }
}
