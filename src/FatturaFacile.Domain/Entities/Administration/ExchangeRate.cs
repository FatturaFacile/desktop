using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Administration;

public sealed class ExchangeRate : Entity<Guid>
{
    public Currency BaseCurrency { get; private set; }
    public Currency TargetCurrency { get; private set; }
    public decimal Rate { get; private set; }
    public DateTime EffectiveDate { get; private set; }

    /// <summary>
    /// ECB (European Central Bank) or SNB (Swiss National Bank)
    /// </summary>
    public string Source { get; private set; } = "ECB";

    private ExchangeRate()
    {
    }

    public static ExchangeRate Create(Currency baseCurr, Currency targetCurr, decimal rate, DateTime date, string source = "ECB")
    {
        return new ExchangeRate
        {
            Id = Guid.NewGuid(),
            BaseCurrency = baseCurr,
            TargetCurrency = targetCurr,
            Rate = rate,
            EffectiveDate = date,
            Source = source,
        };
    }
}