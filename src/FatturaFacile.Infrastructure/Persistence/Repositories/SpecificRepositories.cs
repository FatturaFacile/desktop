using FatturaFacile.Domain.Entities.Administration;
using FatturaFacile.Domain.Entities.Automotive;
using FatturaFacile.Domain.Entities.Crm;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Entities.TimeTracking;
using FatturaFacile.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

public sealed class ClientRepository(FatturaFacileDbContext dbContext) : Repository<Client, Guid>(dbContext), IClientRepository
{
    public Task<Client?> GetByVatOrFiscalCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(
            c =>
                (c.FiscalCode != null && c.FiscalCode.Value == code) ||
                (c.VatNumber != null && c.VatNumber.Value == code) ||
                c.SwissUid == code, cancellationToken);
    }

    public async Task<IReadOnlyList<Client>> GetActiveClientsAsync(CancellationToken cancellationToken = default) => await DbSet.Where(c => c.IsActive).ToListAsync(cancellationToken);
}

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

public sealed class SwissQrBillRepository(FatturaFacileDbContext dbContext) : Repository<SwissQrBill, Guid>(dbContext), ISwissQrBillRepository
{
    public Task<SwissQrBill?> GetByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken = default) => DbSet.FirstOrDefaultAsync(b => b.ReferenceNumber == referenceNumber, cancellationToken);

    public async Task<IReadOnlyList<SwissQrBill>> GetUnpaidBillsAsync(CancellationToken cancellationToken = default) => await DbSet.Where(b => !b.IsPaid).ToListAsync(cancellationToken);
}

public sealed class TireRecordRepository(FatturaFacileDbContext dbContext) : Repository<TireHotelRecord, Guid>(dbContext), ITireRecordRepository
{
    public async Task<IReadOnlyList<TireHotelRecord>> GetInStorageByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default) => await DbSet.Where(t => t.VehicleId == vehicleId && t.Status == "InStorage").ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TireHotelRecord>> GetAllInStorageAsync(CancellationToken cancellationToken = default) => await DbSet.Where(t => t.Status == "InStorage").ToListAsync(cancellationToken);
}

public sealed class TimeEntryRepository(FatturaFacileDbContext dbContext) : Repository<TimeEntry, Guid>(dbContext), ITimeEntryRepository
{
    public async Task<IReadOnlyList<TimeEntry>> GetEntriesByEmployeeAndDateRangeAsync(Guid employeeId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.EmployeeId == employeeId && t.ClockInUtc >= startUtc && t.ClockInUtc <= endUtc)
            .OrderByDescending(t => t.ClockInUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<TimeEntry?> GetActiveClockInAsync(Guid employeeId, CancellationToken cancellationToken = default) => DbSet.FirstOrDefaultAsync(t => t.EmployeeId == employeeId && !t.ClockOutUtc.HasValue, cancellationToken);
}

public sealed class UserRepository(FatturaFacileDbContext dbContext) : Repository<User, Guid>(dbContext), IUserRepository
{
    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) => DbSet.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase), cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => DbSet.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase), cancellationToken);
}

public sealed class TenantRepository(FatturaFacileDbContext dbContext) : Repository<Tenant, Guid>(dbContext), ITenantRepository
{
    public Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) => DbSet.FirstOrDefaultAsync(t => t.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase), cancellationToken);

    public async Task<IReadOnlyList<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default) => await DbSet.Where(t => t.IsActive).ToListAsync(cancellationToken);
}

public sealed class TranslationRepository(FatturaFacileDbContext dbContext) : Repository<AppTranslation, Guid>(dbContext), ITranslationRepository
{
    public async Task<IReadOnlyList<AppTranslation>> GetByLocaleAsync(string locale, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.Locale == locale && t.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    public Task<AppTranslation?> GetByKeyAsync(string locale, string key, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        return DbSet
            .FirstOrDefaultAsync(t => t.Locale == locale && t.Key == key && t.TenantId == tenantId, cancellationToken);
    }
}
