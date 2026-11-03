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
/// <remarks>
/// Initializes a new instance of the <see cref="ContactRepository"/> class.
/// </remarks>
/// <param name="context">The database context.</param>
public class ContactRepository(AppDbContext context) : RepositoryBase<Contact>(context), IContactRepository
{


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
