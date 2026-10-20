using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.RetailPos;

public sealed class PosTerminalSession : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }

    /// <summary>
    /// POS-01
    /// </summary>
    public string TerminalCode { get; private set; } = string.Empty;
    public Guid CashierUserId { get; private set; }
    public DateTime OpenedAtUtc { get; private set; }
    public DateTime? ClosedAtUtc { get; private set; }
    public Money InitialCashFloat { get; private set; } = Money.Euro(100);
    public Money ClosingCashCalculated { get; private set; } = Money.Euro(0);
    public Money CashDiscrepancy { get; private set; } = Money.Euro(0);
    public bool IsClosed { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private PosTerminalSession()
    {
    }

    public static PosTerminalSession Open(Guid tenantId, string terminalCode, Guid cashierId, Money cashFloat)
    {
        return new PosTerminalSession
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TerminalCode = terminalCode.Trim().ToUpperInvariant(),
            CashierUserId = cashierId,
            OpenedAtUtc = DateTime.UtcNow,
            InitialCashFloat = cashFloat,
            IsClosed = false,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void CloseSession(Money countedCash, Money calculatedCash)
    {
        ClosedAtUtc = DateTime.UtcNow;
        ClosingCashCalculated = calculatedCash;
        CashDiscrepancy = countedCash - calculatedCash;
        IsClosed = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}