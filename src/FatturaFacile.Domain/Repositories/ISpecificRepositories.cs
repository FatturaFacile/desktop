using FatturaFacile.Domain.Entities.Administration;
using FatturaFacile.Domain.Entities.Automotive;
using FatturaFacile.Domain.Entities.Crm;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Entities.TimeTracking;

namespace FatturaFacile.Domain.Repositories;

public interface IClientRepository : IRepository<Client, Guid>
{
    public Task<Client?> GetByVatOrFiscalCodeAsync(string code, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<Client>> GetActiveClientsAsync(CancellationToken cancellationToken = default);
}

public interface IItalianInvoiceRepository : IRepository<ItalianElectronicInvoice, Guid>
{
    public Task<ItalianElectronicInvoice?> GetByDocumentNumberAsync(string documentNumber, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<ItalianElectronicInvoice>> GetPendingSdiAsync(CancellationToken cancellationToken = default);
}

public interface ISwissQrBillRepository : IRepository<SwissQrBill, Guid>
{
    public Task<SwissQrBill?> GetByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<SwissQrBill>> GetUnpaidBillsAsync(CancellationToken cancellationToken = default);
}

public interface ITireRecordRepository : IRepository<TireHotelRecord, Guid>
{
    public Task<IReadOnlyList<TireHotelRecord>> GetInStorageByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<TireHotelRecord>> GetAllInStorageAsync(CancellationToken cancellationToken = default);
}

public interface ITimeEntryRepository : IRepository<TimeEntry, Guid>
{
    public Task<IReadOnlyList<TimeEntry>> GetEntriesByEmployeeAndDateRangeAsync(Guid employeeId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default);
    public Task<TimeEntry?> GetActiveClockInAsync(Guid employeeId, CancellationToken cancellationToken = default);
}

public interface IUserRepository : IRepository<User, Guid>
{
    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}

public interface ITenantRepository : IRepository<Tenant, Guid>
{
    public Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default);
}

public interface ITranslationRepository : IRepository<AppTranslation, Guid>
{
    public Task<IReadOnlyList<AppTranslation>> GetByLocaleAsync(string locale, Guid? tenantId = null, CancellationToken cancellationToken = default);
    public Task<AppTranslation?> GetByKeyAsync(string locale, string key, Guid? tenantId = null, CancellationToken cancellationToken = default);
}
