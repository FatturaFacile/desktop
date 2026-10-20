using System.Globalization;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Purchasing;

public sealed class Iso20022PaymentInstruction : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string InstructionId { get; private set; } = string.Empty;
    public string EndToEndId { get; private set; } = string.Empty;
    public DateTime ExecutionDate { get; private set; }
    public string DebtorIban { get; private set; } = string.Empty;
    public string CreditorIban { get; private set; } = string.Empty;
    public string CreditorName { get; private set; } = string.Empty;
    public Money Amount { get; private set; } = Money.Euro(0);
    public string RemittanceInformation { get; private set; } = string.Empty;

    /// <summary>
    /// Draft, GeneratedPain001, Executed
    /// </summary>
    public string Status { get; private set; } = "Draft";

    private Iso20022PaymentInstruction()
    {
    }

    public static Iso20022PaymentInstruction Create(
        Guid tenantId,
        string debtorIban,
        string creditorIban,
        string creditorName,
        Money amount,
        string remittance,
        DateTime executionDate)
    {
        var id = Guid.NewGuid();
        return new Iso20022PaymentInstruction
        {
            Id = id,
            TenantId = tenantId,
            InstructionId = string.Create(CultureInfo.InvariantCulture, $"PMT-{DateTime.UtcNow:yyyyMMdd}-{id.ToString()[..8]}"),
            EndToEndId = string.Create(CultureInfo.InvariantCulture, $"E2E-{DateTime.UtcNow:yyyyMMddHHmmss}"),
            DebtorIban = debtorIban.Replace(" ", string.Empty, StringComparison.Ordinal),
            CreditorIban = creditorIban.Replace(" ", string.Empty, StringComparison.Ordinal),
            CreditorName = creditorName,
            Amount = amount,
            RemittanceInformation = remittance,
            ExecutionDate = executionDate,
            Status = "Draft",
        };
    }

    public void MarkPain001Generated() => Status = "GeneratedPain001";
}