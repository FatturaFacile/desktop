// <copyright file="Iso20022PaymentInstruction.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Purchasing;

/// <summary>
/// Represents an ISO 20022 Credit Transfer payment order (SEPA or Swiss pain.001)
/// used to execute outbound supplier payments.
/// </summary>
public sealed class Iso20022PaymentInstruction : Entity<Guid>, ITenantEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Iso20022PaymentInstruction"/> class.
    /// Required by Entity Framework.
    /// </summary>
    private Iso20022PaymentInstruction()
    {
    }

    /// <summary>
    /// Gets or sets the unique tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the ISO 20022 instruction identifier (&lt;InstrId&gt;).
    /// </summary>
    public string InstructionId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the ISO 20022 end-to-end identifier (&lt;EndToEndId&gt;).
    /// </summary>
    public string EndToEndId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the requested execution date.
    /// </summary>
    public DateTime ExecutionDate { get; private set; }

    /// <summary>
    /// Gets the debtor IBAN.
    /// </summary>
    public string DebtorIban { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the creditor IBAN or QR-IBAN.
    /// </summary>
    public string CreditorIban { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the creditor name.
    /// </summary>
    public string CreditorName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the transfer amount.
    /// </summary>
    public Money Amount { get; private set; } = Money.Euro(0);

    /// <summary>
    /// Gets the remittance information.
    /// </summary>
    public string RemittanceInformation { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the current status.
    /// </summary>
    public PaymentInstructionStatus Status { get; private set; }
        = PaymentInstructionStatus.Draft;

    /// <summary>
    /// Creates a new payment instruction.
    /// </summary>
    /// <param name="tenantId">Tenant identifier.</param>
    /// <param name="debtorIban">Debtor IBAN.</param>
    /// <param name="creditorIban">Creditor IBAN.</param>
    /// <param name="creditorName">Creditor name.</param>
    /// <param name="amount">Payment amount.</param>
    /// <param name="remittance">Remittance information.</param>
    /// <param name="executionDate">Execution date.</param>
    /// <returns>A configured payment instruction.</returns>
    public static Iso20022PaymentInstruction Create(
        Guid tenantId,
        string debtorIban,
        string creditorIban,
        string creditorName,
        Money amount,
        string remittance,
        DateTime executionDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(debtorIban);
        ArgumentException.ThrowIfNullOrWhiteSpace(creditorIban);
        ArgumentException.ThrowIfNullOrWhiteSpace(creditorName);
        ArgumentNullException.ThrowIfNull(amount);

        var id = Guid.NewGuid();

        return new Iso20022PaymentInstruction
        {
            Id = id,
            TenantId = tenantId,
            InstructionId = FormattableString.Invariant(
                $"PMT-{DateTime.UtcNow:yyyyMMdd}-{id:N}"[..17]),
            EndToEndId = FormattableString.Invariant(
                $"E2E-{DateTime.UtcNow:yyyyMMddHHmmss}"),
            DebtorIban = NormalizeIban(debtorIban),
            CreditorIban = NormalizeIban(creditorIban),
            CreditorName = creditorName.Trim(),
            Amount = amount,
            RemittanceInformation = remittance?.Trim() ?? string.Empty,
            ExecutionDate = executionDate.Date,
            Status = PaymentInstructionStatus.Draft,
        };
    }

    /// <summary>
    /// Marks the instruction as exported to a pain.001 file.
    /// </summary>
    public void MarkPain001Generated()
    {
        Status = PaymentInstructionStatus.GeneratedPain001;
    }

    /// <summary>
    /// Marks the instruction as executed.
    /// </summary>
    public void MarkExecuted()
    {
        Status = PaymentInstructionStatus.Executed;
    }

    /// <summary>
    /// Marks the instruction as rejected.
    /// </summary>
    public void MarkRejected()
    {
        Status = PaymentInstructionStatus.Rejected;
    }

    private static string NormalizeIban(string iban) =>
        iban.Replace(
            " ",
            string.Empty,
            StringComparison.Ordinal)
            .ToUpperInvariant();
}
