// <copyright file="Iso20022PaymentInstruction.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Purchasing;

/// <summary>
/// Represents an ISO 20022 Credit Transfer payment order (such as SEPA or Swiss pain.001.001.03 / pain.001.001.09)
/// issued to execute outbound supplier payments from a corporate bank account.
/// </summary>
public sealed class Iso20022PaymentInstruction : Entity<Guid>, ITenantEntity
{
    private Iso20022PaymentInstruction()
    {
    }

    /// <summary>
    /// Gets or sets the unique tenant identifier for multi-tenant data partitioning.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique point-to-point payment instruction identification assigned by the initiating party (ISO 20022 tag InstructionId / &lt;InstrId&gt;).
    /// </summary>
    public string InstructionId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the unique end-to-end identification passed unchanged throughout the clearing chain to reconcile the payment (ISO 20022 tag EndToEndId / &lt;EndToEndId&gt;).
    /// </summary>
    public string EndToEndId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the requested execution date (ISO 20022 tag RequestedExecutionDate / &lt;ReqdExctnDt&gt;) on which the debtor's account should be debited.
    /// </summary>
    public DateTime ExecutionDate { get; private set; }

    /// <summary>
    /// Gets the International Bank Account Number (IBAN) of the debtor initiating the transfer.
    /// </summary>
    public string DebtorIban { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the International Bank Account Number (IBAN) or Swiss QR-IBAN of the recipient creditor.
    /// </summary>
    public string CreditorIban { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the legal name or company trade name of the recipient creditor (payee).
    /// </summary>
    public string CreditorName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the monetary amount and currency to be transferred to the creditor.
    /// </summary>
    public Money Amount { get; private set; } = Money.Euro(0);

    /// <summary>
    /// Gets the remittance information (such as structured SCOR/CBI reference or unstructured invoice reference) transmitted with the transfer.
    /// </summary>
    public string RemittanceInformation { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the current processing status of the payment instruction (e.g. "Draft", "GeneratedPain001", or "Executed").
    /// </summary>
    public string Status { get; private set; } = "Draft";

    /// <summary>
    /// Creates a new <see cref="Iso20022PaymentInstruction"/> with generated instruction and end-to-end identifiers.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="debtorIban">The IBAN of the ordering account.</param>
    /// <param name="creditorIban">The IBAN or QR-IBAN of the beneficiary.</param>
    /// <param name="creditorName">The full name of the beneficiary.</param>
    /// <param name="amount">The monetary sum to transfer.</param>
    /// <param name="remittance">The payment purpose, invoice number, or structured reference.</param>
    /// <param name="executionDate">The requested bank debit execution date.</param>
    /// <returns>A newly configured <see cref="Iso20022PaymentInstruction"/> ready for pain.001 XML batch generation.</returns>
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
    /// Updates the instruction status to indicate that it has been bundled into an exported ISO 20022 pain.001 XML file for bank submission.
    /// </summary>
    public void MarkPain001Generated() => Status = "GeneratedPain001";
}
