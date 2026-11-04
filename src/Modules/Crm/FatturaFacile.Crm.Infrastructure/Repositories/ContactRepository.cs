using Microsoft.EntityFrameworkCore;
using FatturaFacile.Crm.Domain.Aggregates;
using FatturaFacile.Crm.Domain.Repositories;
using FatturaFacile.Crm.Infrastructure.Persistence;

namespace FatturaFacile.Crm.Infrastructure.Repositories;

public sealed class ContactRepository : IContactRepository
{
    private readonly CrmDbContext _context;

    public ContactRepository(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task AddAsync(Contact contact, CancellationToken cancellationToken = default)
    {
        await _context.Contacts.AddAsync(contact, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Contact contact, CancellationToken cancellationToken = default)
    {
        _context.Contacts.Update(contact);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
