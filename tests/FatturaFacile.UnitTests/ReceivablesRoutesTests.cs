// <copyright file="ReceivablesRoutesTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Receivables;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the receivables routes and of the query strings they build.
/// </summary>
public sealed class ReceivablesRoutesTests
{
    /// <summary>
    /// The default filter asks for the open items, 50 per page, first page.
    /// </summary>
    [Fact]
    public void Items_DefaultFilter_ListsOpenItemsOfTheFirstPage() =>
        Assert.Equal("invoicing/receivables/items?open_only=1&per_page=50&page=1", ReceivablesRoutes.Items(ReceivableItemFilter.Default));

    /// <summary>
    /// The drill-down of an aging bucket is the bucket and the customer in the query.
    /// </summary>
    [Fact]
    public void Items_DrillDown_CarriesTheBucketAndTheCustomer()
    {
        var customer = new Guid(0xC0C0C0C0, 0xC0C0, 0xC0C0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0);
        var filter = ReceivableItemFilter.Default with { CustomerId = customer, Bucket = "31_60", Page = 3 };

        var route = ReceivablesRoutes.Items(filter);

        Assert.Equal("invoicing/receivables/items?customer_id=c0c0c0c0-c0c0-c0c0-c0c0-c0c0c0c0c0c0&bucket=31_60&open_only=1&per_page=50&page=3", route);
    }

    /// <summary>
    /// Dates and amounts use the invariant culture and the values are escaped.
    /// </summary>
    [Fact]
    public void Items_AllFilters_AreEncodedWithTheInvariantCulture()
    {
        var filter = new ReceivableItemFilter(null, ReceivableStatus.PartiallyPaid, "credit note", "CHF", new DateOnly(2027, 1, 5), new DateOnly(2027, 2, 6), null, null, 10.5m, 99m, null, OpenOnly: false, 1, 25);

        var route = ReceivablesRoutes.Items(filter);

        Assert.Equal("invoicing/receivables/items?status=partially_paid&document_type=credit%20note&currency=CHF&due_from=2027-01-05&due_to=2027-02-06&value_from=10.50&value_to=99.00&open_only=0&per_page=25&page=1", route);
    }

    /// <summary>
    /// The currency is only sent when it is given.
    /// </summary>
    [Fact]
    public void Dashboard_WithAndWithoutCurrency()
    {
        Assert.Equal("invoicing/receivables/dashboard", ReceivablesRoutes.Dashboard(null));
        Assert.Equal("invoicing/receivables/dashboard?currency=CHF", ReceivablesRoutes.Dashboard("CHF"));
        Assert.Equal("invoicing/receivables/dashboard", ReceivablesRoutes.Dashboard(string.Empty));
    }

    /// <summary>
    /// The customer scoped routes follow the contract.
    /// </summary>
    [Fact]
    public void CustomerRoutes_UseTheContractPaths()
    {
        var id = new Guid(0xC0C0C0C0, 0xC0C0, 0xC0C0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0);
        const string root = "invoicing/receivables/customers/c0c0c0c0-c0c0-c0c0-c0c0-c0c0c0c0c0c0";

        Assert.Equal(root + "/credit", ReceivablesRoutes.Credit(id));
        Assert.Equal(root + "/risk", ReceivablesRoutes.Risk(id));
        Assert.Equal(root + "/credit-limits", ReceivablesRoutes.CreditLimits(id));
        Assert.Equal(root + "/unreconciled-payments", ReceivablesRoutes.UnreconciledPayments(id));
        Assert.Equal(root + "/overrides", ReceivablesRoutes.Overrides(id));
        Assert.Equal(root + "/statement/send", ReceivablesRoutes.StatementSend(id));
        Assert.Equal(root + "/ledger?from=2027-01-01&to=2027-03-31", ReceivablesRoutes.Ledger(id, new DateOnly(2027, 1, 1), new DateOnly(2027, 3, 31), null));
        Assert.Equal(root + "/statement?from=2027-01-01&to=2027-03-31&currency=EUR", ReceivablesRoutes.Statement(id, new DateOnly(2027, 1, 1), new DateOnly(2027, 3, 31), "EUR"));
        Assert.Equal(root + "/statement/xlsx?from=2027-01-01&to=2027-03-31", ReceivablesRoutes.StatementFile(id, "xlsx", new DateOnly(2027, 1, 1), new DateOnly(2027, 3, 31), null));
    }

    /// <summary>
    /// The write routes follow the contract.
    /// </summary>
    [Fact]
    public void ActionRoutes_UseTheContractPaths()
    {
        var id = new Guid(0x11111111, 0x1111, 0x1111, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11);

        Assert.Equal("invoicing/receivables/allocations", ReceivablesRoutes.Allocations);
        Assert.Equal("invoicing/receivables/allocations/11111111-1111-1111-1111-111111111111/reverse", ReceivablesRoutes.ReverseAllocation(id));
        Assert.Equal("invoicing/receivables/items/11111111-1111-1111-1111-111111111111/dispute", ReceivablesRoutes.ItemDispute(id));
        Assert.Equal("invoicing/receivables/credit-limits/11111111-1111-1111-1111-111111111111", ReceivablesRoutes.CreditLimit(id));
        Assert.Equal("invoicing/receivables/overrides/11111111-1111-1111-1111-111111111111/revoke", ReceivablesRoutes.RevokeOverride(id));
        Assert.Equal("invoicing/receivables/provision/rates", ReceivablesRoutes.ProvisionRatesRoute);
    }

    /// <summary>
    /// The credit check sends the scope and a two-decimal amount; an unspecific scope asks for the order check.
    /// </summary>
    [Fact]
    public void CreditCheck_AndAggregates_FormatAmountsAndScope()
    {
        var id = new Guid(0xC0C0C0C0, 0xC0C0, 0xC0C0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0);

        Assert.Equal("invoicing/receivables/credit-check?customer_id=c0c0c0c0-c0c0-c0c0-c0c0-c0c0c0c0c0c0&scope=delivery_note&amount=1250.50", ReceivablesRoutes.CreditCheck(id, CreditScope.DeliveryNote, 1250.5m));
        Assert.Contains("scope=order", ReceivablesRoutes.CreditCheck(id, CreditScope.All, 0m), StringComparison.Ordinal);
        Assert.Equal("invoicing/receivables/dso?period_days=90", ReceivablesRoutes.Dso(null, 90, null));
        Assert.Equal("invoicing/receivables/provision?currency=CHF", ReceivablesRoutes.Provision(null, "CHF"));
        Assert.Equal("invoicing/receivables/aging?customer_id=c0c0c0c0-c0c0-c0c0-c0c0-c0c0c0c0c0c0", ReceivablesRoutes.Aging(id, null));
    }
}
