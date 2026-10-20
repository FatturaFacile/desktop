using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

public sealed class ItalianInvoiceRepository(FatturaFacileDbContext dbContext) : Repository<ItalianElectronicInvoice, Guid>(dbContext), IItalianInvoiceRepository
{
    public Task<ItalianElectronicInvoice?> GetByDocumentNumberAsync(string documentNumber, CancellationToken cancellationToken = default)
    {
        return DbSet.Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.DocumentNumber == documentNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<ItalianElectronicInvoice>> GetPendingSdiAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.Include(i => i.Lines)
            .Where(i => i.SdiStatus == Domain.Enums.SdiTransmissionStatus.Draft || i.SdiStatus == Domain.Enums.SdiTransmissionStatus.ReadyToSend)
            .ToListAsync(cancellationToken);
    }

    public override async Task<IReadOnlyList<ItalianElectronicInvoice>> GetAllAsync(CancellationToken cancellationToken = default) => await DbSet.Include(i => i.Lines).ToListAsync(cancellationToken);
}