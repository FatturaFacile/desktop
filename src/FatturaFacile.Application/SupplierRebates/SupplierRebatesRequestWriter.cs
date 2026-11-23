// <copyright file="SupplierRebatesRequestWriter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// Builds the JSON bodies of the supplier rebate requests: dates as `yyyy-MM-dd`, decimals as JSON numbers.
/// </summary>
public static class SupplierRebatesRequestWriter
{
    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>
    /// Builds the body of an agreement create or update.
    /// </summary>
    /// <param name="input">The agreement.</param>
    /// <returns>The JSON body.</returns>
    public static string Agreement(RebateAgreementInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return Build(writer =>
        {
            writer.WriteString("code", input.Code);
            writer.WriteString("name", input.Name);
            writer.WriteString("direction", RebateCodes.DirectionPurchase);
            writer.WriteString("contact_id", input.ContactId);
            writer.WriteString("tier_mode", RebateWire.ToWire(input.TierMode));
            writer.WriteString("starts_on", Format(input.StartsOn));
            writer.WriteString("ends_on", Format(input.EndsOn));
            writer.WriteString("currency", input.Currency);
            writer.WriteString("rebate_type", input.RebateType);
            WriteText(writer, "supplier_contract_id", input.SupplierContractId);
            WriteNumber(writer, "fixed_bonus_target", input.FixedBonusTarget);
            WriteNumber(writer, "fixed_bonus_amount", input.FixedBonusAmount);
            writer.WriteString("accrual_frequency", input.AccrualFrequency);
            WriteText(writer, "accrual_account_id", input.AccrualAccountId);
            WriteText(writer, "income_account_id", input.IncomeAccountId);
            WriteText(writer, "notes", input.Notes);
            if (input.Version is { } version)
            {
                writer.WriteNumber("version", version);
            }

            writer.WriteStartArray("tiers");
            foreach (var tier in input.Tiers)
            {
                writer.WriteStartObject();
                writer.WriteNumber("threshold_amount", tier.ThresholdAmount);
                writer.WriteNumber("rate_percent", tier.RatePercent);
                WriteNumber(writer, "flat_amount", tier.FlatAmount);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        });
    }

    /// <summary>
    /// Builds the body of a contribution create or update.
    /// </summary>
    /// <param name="input">The contribution.</param>
    /// <returns>The JSON body.</returns>
    public static string Contribution(RebateContributionInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return Build(writer =>
        {
            writer.WriteString("kind", input.Kind);
            writer.WriteString("description", input.Description);
            writer.WriteNumber("amount", input.Amount);
            writer.WriteString("due_on", Format(input.DueOn));
        });
    }

    /// <summary>
    /// Builds the body of an accrual run.
    /// </summary>
    /// <param name="periodEnd">The end of the period (the server uses today when it is null).</param>
    /// <param name="periodStart">The start of the period (the server derives it when it is null).</param>
    /// <returns>The JSON body.</returns>
    public static string Accrual(DateOnly? periodEnd, DateOnly? periodStart) => Build(writer =>
    {
        if (periodEnd is { } end)
        {
            writer.WriteString("period_end", Format(end));
        }

        if (periodStart is { } start)
        {
            writer.WriteString("period_start", Format(start));
        }
    });

    /// <summary>
    /// Builds the body of the reversal of an accrual run.
    /// </summary>
    /// <param name="reason">The reason.</param>
    /// <returns>The JSON body.</returns>
    public static string Reversal(string reason) => Build(writer => writer.WriteString("reason", reason));

    /// <summary>
    /// Builds the body of a settlement proposal.
    /// </summary>
    /// <param name="input">The proposal.</param>
    /// <returns>The JSON body.</returns>
    public static string Proposal(RebateProposalInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return Build(writer =>
        {
            writer.WriteString("execution_mode", RebateWire.ToWire(input.Mode));
            WriteNumber(writer, "tax_rate", input.TaxRate);
            WriteText(writer, "passive_invoice_id", input.PassiveInvoiceId);
            WriteText(writer, "notes", input.Notes);
        });
    }

    /// <summary>
    /// Builds the body of a settlement workflow step.
    /// </summary>
    /// <param name="details">The step data.</param>
    /// <returns>The JSON body.</returns>
    public static string SettlementStep(RebateSettlementStep details)
    {
        ArgumentNullException.ThrowIfNull(details);

        return Build(writer =>
        {
            if (details.Version is { } version)
            {
                writer.WriteNumber("version", version);
            }

            if (details.ExecutedOn is { } executedOn)
            {
                writer.WriteString("executed_on", Format(executedOn));
            }

            if (!string.IsNullOrEmpty(details.PayablesAccountId))
            {
                writer.WriteString("payables_account_id", details.PayablesAccountId);
            }

            if (!string.IsNullOrEmpty(details.VatAccountId))
            {
                writer.WriteString("vat_account_id", details.VatAccountId);
            }
        });
    }

    private static string Format(DateOnly date) => date.ToString(DateFormat, CultureInfo.InvariantCulture);

    private static void WriteText(Utf8JsonWriter writer, string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            writer.WriteNull(name);
        }
        else
        {
            writer.WriteString(name, value);
        }
    }

    private static void WriteNumber(Utf8JsonWriter writer, string name, decimal? value)
    {
        if (value is { } number)
        {
            writer.WriteNumber(name, number);
        }
        else
        {
            writer.WriteNull(name);
        }
    }

    private static string Build(Action<Utf8JsonWriter> fill)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            fill(writer);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }
}
