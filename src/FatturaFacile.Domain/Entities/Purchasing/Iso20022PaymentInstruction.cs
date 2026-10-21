// <copyright file="Iso20022PaymentInstruction.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Purchasing;

/// <summary>
/// Represents iso20022 payment instruction.
/// </summary>
public sealed class Iso20022PaymentInstruction : Entity<Guid>, ITenantEntity
{
    private Iso20022PaymentInstruction()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the instruction id.
    /// </summary>
    public string InstructionId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the end to end id.
    /// </summary>
    public string EndToEndId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the execution date.
    /// </summary>
    public DateTime ExecutionDate { get; private set; }

    /// <summary>
    /// Gets the debtor iban.
    /// </summary>
    public string DebtorIban { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the creditor iban.
    /// </summary>
    public string CreditorIban { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the creditor name.
    /// </summary>
    public string CreditorName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the amount.
    /// </summary>
    public Money Amount { get; private set; } = Money.Euro(0);

    /// <summary>
    /// Gets the remittance information.
    /// </summary>
    public string RemittanceInformation { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the draft, GeneratedPain001, Executed.
    /// </summary>
    public string Status { get; private set; } = "Draft";

    /// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="debtorIban">The debtor iban.</param>
    /// <param name="creditorIban">The creditor iban.</param>
    /// <param name="creditorName">The creditor name.</param>
    /// <param name="amount">The amount.</param>
    /// <param name="remittance">The remittance.</param>
    /// <param name="executionDate">The execution date.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
            InstructionId = string.Create(
                CultureInfo.InvariantCulture,
                $"PMT-{DateTime.UtcNow:yyyyMMdd}-{id.ToString()[..8]}"),
            EndToEndId = string.Create(
                CultureInfo.InvariantCulture,
                $"E2E-{DateTime.UtcNow:yyyyMMddHHmmss}"),
            DebtorIban = debtorIban?.Replace(
                " ",
                string.Empty,
                StringComparison.Ordinal) ?? string.Empty,
            CreditorIban = creditorIban?.Replace(
                " ",
                string.Empty,
                StringComparison.Ordinal) ?? string.Empty,
            CreditorName = creditorName ?? string.Empty,
            Amount = amount,
            RemittanceInformation = remittance ?? string.Empty,
            ExecutionDate = executionDate,
            Status = "Draft",
        };
    }

    /// <summary>
    /// Mark Pain001 Generated.
    /// </summary>
    public void MarkPain001Generated() => Status = "GeneratedPain001";
}
