using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Advisory;

public sealed class BillableTimeEntry : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid ClientMatterId { get; private set; }
    public Guid ProfessionalUserId { get; private set; }
    public DateTime ActivityDate { get; private set; }
    public int MinutesSpent { get; private set; }
    public decimal HourlyRate { get; private set; }
    public Currency Currency { get; private set; } = Currency.EUR;
    public string Description { get; private set; } = string.Empty;
    public bool IsInvoiced { get; private set; }

    public decimal TotalFee => decimal.Round(MinutesSpent / 60m * HourlyRate, 2, MidpointRounding.AwayFromZero);

    private BillableTimeEntry()
    {
    }

    public static BillableTimeEntry Create(
        Guid tenantId,
        Guid matterId,
        Guid userId,
        DateTime date,
        int minutes,
        decimal rate,
        Currency currency,
        string description)
    {
        return new BillableTimeEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ClientMatterId = matterId,
            ProfessionalUserId = userId,
            ActivityDate = date,
            MinutesSpent = minutes,
            HourlyRate = rate,
            Currency = currency,
            Description = description.Trim(),
            IsInvoiced = false,
        };
    }

    public void MarkInvoiced() => IsInvoiced = true;
}