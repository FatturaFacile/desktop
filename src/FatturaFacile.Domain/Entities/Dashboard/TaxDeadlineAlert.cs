using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Dashboard;

public sealed class TaxDeadlineAlert : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime DueDate { get; private set; }
    public Money EstimatedAmount { get; private set; } = Money.Euro(0);
    public Jurisdiction Jurisdiction { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedDateUtc { get; private set; }

    private TaxDeadlineAlert()
    {
    }

    public static TaxDeadlineAlert Create(
        Guid tenantId,
        string title,
        string description,
        DateTime dueDate,
        Money estimatedAmount,
        Jurisdiction jurisdiction)
    {
        return new TaxDeadlineAlert
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Title = title,
            Description = description,
            DueDate = dueDate,
            EstimatedAmount = estimatedAmount,
            Jurisdiction = jurisdiction,
            IsCompleted = false,
        };
    }

    public void MarkCompleted()
    {
        IsCompleted = true;
        CompletedDateUtc = DateTime.UtcNow;
    }
}