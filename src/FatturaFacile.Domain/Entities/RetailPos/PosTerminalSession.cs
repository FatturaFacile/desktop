using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.RetailPos;

/// <summary>
/// Represents pos terminal session.
/// </summary>
public sealed class PosTerminalSession : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the pOS-01.
    /// </summary>
    public string TerminalCode { get; private set; } = string.Empty;

/// <summary>
    /// Gets the cashier user id.
    /// </summary>
    public Guid CashierUserId { get; private set; }

/// <summary>
    /// Gets the opened at utc.
    /// </summary>
    public DateTime OpenedAtUtc { get; private set; }

/// <summary>
    /// Gets the closed at utc.
    /// </summary>
    public DateTime? ClosedAtUtc { get; private set; }

/// <summary>
    /// Gets the initial cash float.
    /// </summary>
    public Money InitialCashFloat { get; private set; } = Money.Euro(100);

/// <summary>
    /// Gets the closing cash calculated.
    /// </summary>
    public Money ClosingCashCalculated { get; private set; } = Money.Euro(0);

/// <summary>
    /// Gets the cash discrepancy.
    /// </summary>
    public Money CashDiscrepancy { get; private set; } = Money.Euro(0);

/// <summary>
    /// Gets a value indicating whether closed.
    /// </summary>
    public bool IsClosed { get; private set; }

/// <summary>
    /// Gets or sets the created at utc.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

/// <summary>
    /// Gets or sets the created by.
    /// </summary>
    public string? CreatedBy { get; set; }

/// <summary>
    /// Gets or sets the updated at utc.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

/// <summary>
    /// Gets or sets the updated by.
    /// </summary>
    public string? UpdatedBy { get; set; }

private PosTerminalSession()
    {
    }

/// <summary>
    /// Open.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="terminalCode">The terminal code.</param>
    /// <param name="cashierId">The cashier id.</param>
    /// <param name="cashFloat">The cash float.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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

/// <summary>
    /// Close Session.
    /// </summary>
    /// <param name="countedCash">The counted cash.</param>
    /// <param name="calculatedCash">The calculated cash.</param>
    public void CloseSession(Money countedCash, Money calculatedCash)
    {
        ClosedAtUtc = DateTime.UtcNow;
        ClosingCashCalculated = calculatedCash;
        CashDiscrepancy = countedCash - calculatedCash;
        IsClosed = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
