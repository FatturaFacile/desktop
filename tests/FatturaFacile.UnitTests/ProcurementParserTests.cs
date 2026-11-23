// <copyright file="ProcurementParserTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Application.Procurement;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the parser of the purchase order fulfilment answers (Mod 046) and of the status rules and routes.
/// </summary>
public sealed class ProcurementParserTests
{
    private const string OrderJson = """
        {"id":"11111111-1111-1111-1111-111111111111","po_number":"PO-2027-0001","supplier_id":"33333333-3333-3333-3333-333333333333","status":"confirmed",
         "order_date":"2027-03-01","expected_delivery_date":"2027-03-20","promised_delivery_date":"2027-03-25","currency":"CHF","total_amount":"1043.10",
         "is_dropship":true,"sales_order_id":"44444444-4444-4444-4444-444444444444","reminder_count":2,"last_reminder_at":"2027-03-26T08:00:00+00:00",
         "supplier_confirmation_number":"AB-7","dispatch_channel":"email","supplier":{"name":"Rossi SA"},
         "lines":[{"id":"22222222-2222-2222-2222-222222222222","line_number":1,"description":"Screws","quantity_ordered":"10.0000","quantity_received":"2.0000",
          "uom":"BOX","base_uom":"PZ","uom_conversion_factor":"12.000000","quantity_base_ordered":"120.0000","quantity_base_received":"24.0000","quantity_confirmed":"8.0000",
          "discount_cascade":["10.00","5.00"],"discount_percent_effective":"14.50","confirmed_delivery_date":"2027-03-24","promised_delivery_date":"2027-03-25",
          "unit_price":"100.0000","line_net_amount":"855.00"}]}
        """;

    /// <summary>
    /// An order keeps its status, promised date, reminder count, dropship link and the lines with their dual unit of measure and cascade.
    /// </summary>
    [Fact]
    public void ParseOrders_ReadsStatusPromisedDateAndLines()
    {
        var order = Assert.Single(ProcurementParser.ParseOrders("{\"data\":[" + OrderJson + "]}"));

        Assert.Equal("confirmed", order.Status);
        Assert.Equal("PO-2027-0001", order.Number);
        Assert.Equal("Rossi SA", order.SupplierName);
        Assert.Equal(new DateOnly(2027, 3, 25), order.PromisedDeliveryDate);
        Assert.Equal(2, order.ReminderCount);
        Assert.True(order.IsDropship);
        Assert.Equal(Guid.Parse("44444444-4444-4444-4444-444444444444"), order.SalesOrderId);
        Assert.Equal("AB-7", order.SupplierConfirmationNumber);
        Assert.Equal(1043.10m, order.TotalAmount);

        var line = Assert.Single(order.Lines);
        Assert.Equal("BOX", line.Uom);
        Assert.Equal("PZ", line.BaseUom);
        Assert.Equal(12m, line.UomConversionFactor);
        Assert.Equal(120m, line.QuantityBaseOrdered);
        Assert.Equal(8m, line.QuantityConfirmed);
        Assert.Equal([10m, 5m], line.DiscountCascade);
        Assert.Equal(14.5m, line.DiscountPercentEffective);
        Assert.Equal(new DateOnly(2027, 3, 25), line.PromisedDeliveryDate);
    }

    /// <summary>
    /// A line without a conversion factor is read with factor one, never zero.
    /// </summary>
    [Fact]
    public void ParseOrder_MissingFactor_DefaultsToOne()
    {
        var order = ProcurementParser.ParseOrder("""{"id":"11111111-1111-1111-1111-111111111111","status":"sent","lines":[{"id":"22222222-2222-2222-2222-222222222222","uom":"PZ"}]}""");

        Assert.Equal(1m, Assert.Single(order.Lines).UomConversionFactor);
        Assert.Empty(order.Lines[0].DiscountCascade);
    }

    /// <summary>
    /// The overdue list carries the order, the days late, the late lines and whether the next reminder is due.
    /// </summary>
    [Fact]
    public void ParseOverdue_ReadsDaysLinesAndReminderState()
    {
        var json = "{\"data\":[{\"order\":" + OrderJson + ",\"days_overdue\":12,\"overdue_line_ids\":[\"22222222-2222-2222-2222-222222222222\"],\"reminder_count\":2,\"last_reminder_at\":\"2027-03-26T08:00:00+00:00\",\"next_reminder_due\":true}]}";

        var row = Assert.Single(ProcurementParser.ParseOverdue(json));

        Assert.Equal(12, row.DaysOverdue);
        Assert.True(row.NextReminderDue);
        Assert.Equal(2, row.ReminderCount);
        Assert.Equal(Guid.Parse("22222222-2222-2222-2222-222222222222"), Assert.Single(row.OverdueLineIds));
        Assert.Equal("PO-2027-0001", row.Order.Number);
        Assert.Equal(DateTimeKind.Utc, row.LastReminderAt!.Value.Kind);
    }

    /// <summary>
    /// A supplier confirmation lists its line variances.
    /// </summary>
    [Fact]
    public void ParseConfirmations_ReadsVariances()
    {
        const string Json = """
            {"data":[{"id":"55555555-5555-5555-5555-555555555555","purchase_order_id":"11111111-1111-1111-1111-111111111111","confirmation_number":"AB-7","has_variance":true,"requires_review":true,
             "notes":"Partial","confirmed_at":"2027-03-02","price_variance_total":"12.50","quantity_variance_total":"-2.0000",
             "lines":[{"purchase_order_line_id":"22222222-2222-2222-2222-222222222222","ordered_quantity":"10.0000","confirmed_quantity":"8.0000","ordered_unit_price":"100.0000","confirmed_unit_price":"101.0000",
              "requested_delivery_date":"2027-03-20","confirmed_delivery_date":"2027-03-25","quantity_variance":"-2.0000","quantity_variance_base":"-24.0000","price_variance":"1.0000","price_variance_percent":"1.0000",
              "line_total_variance":"-190.00","delivery_delay_days":5,"variance_reason":"Short stock"}]}]}
            """;

        var confirmation = Assert.Single(ProcurementParser.ParseConfirmations(Json));
        var line = Assert.Single(confirmation.Lines);

        Assert.Equal("AB-7", confirmation.ConfirmationNumber);
        Assert.True(confirmation.HasVariance);
        Assert.True(confirmation.RequiresReview);
        Assert.Equal(new DateOnly(2027, 3, 2), confirmation.ConfirmedAt);
        Assert.Equal(-2m, confirmation.QuantityVarianceTotal);
        Assert.Equal(-24m, line.QuantityVarianceBase);
        Assert.Equal(5, line.DeliveryDelayDays);
        Assert.Equal("Short stock", line.VarianceReason);
    }

    /// <summary>
    /// The dispatch and reminder logs are read with their instants in UTC.
    /// </summary>
    [Fact]
    public void ParseDispatchesAndReminders_ReadTheLogs()
    {
        var dispatch = Assert.Single(ProcurementParser.ParseDispatches("""{"data":[{"id":"66666666-6666-6666-6666-666666666666","channel":"email","status":"sent","recipient":"a@b.ch","message_reference":"M1","dispatched_at":"2027-03-01T10:00:00+00:00"}]}"""));
        var reminder = Assert.Single(ProcurementParser.ParseReminders("""{"data":[{"id":"77777777-7777-7777-7777-777777777777","level":2,"channel":"email","trigger":"manual","days_overdue":9,"sent_at":"2027-03-26T08:00:00+00:00"}]}"""));

        Assert.Equal("email", dispatch.Channel);
        Assert.Equal("a@b.ch", dispatch.Recipient);
        Assert.Equal(new DateTime(2027, 3, 1, 10, 0, 0, DateTimeKind.Utc), dispatch.DispatchedAt);
        Assert.Equal(2, reminder.Level);
        Assert.Equal(9, reminder.DaysOverdue);
    }

    /// <summary>
    /// A dropshipment has its recipient address on one line, its DDT, billing authorisation and virtual stock movements.
    /// </summary>
    [Fact]
    public void ParseDropship_ReadsDdtDeliveryAndMovements()
    {
        const string Json = """
            {"data":{"id":"88888888-8888-8888-8888-888888888888","purchase_order_id":"11111111-1111-1111-1111-111111111111","sales_order_id":"44444444-4444-4444-4444-444444444444","status":"delivered",
             "recipient_name":"Bianchi SA","recipient_street":"Via Roma","recipient_house_number":"5","recipient_postal_code":"6900","recipient_city":"Lugano","recipient_country_code":"CH",
             "ddt_number":"DDT-1","ddt_issued_at":"2027-03-10T09:00:00+00:00","supplier_shipment_reference":"S-1","delivery_proof_reference":"POD-1","billing_authorized":true,
             "billing_authorization_reference":"BA-1","delivered_at":"2027-03-12T09:00:00+00:00",
             "virtual_stock_movements":[{"purchase_order_line_id":"22222222-2222-2222-2222-222222222222","direction":"in","quantity_base":"120.0000","base_uom":"PZ","unit_cost_base":"7.1250","valued_amount":"855.00","moved_at":"2027-03-12T09:00:00+00:00"}]}}
            """;

        var drop = ProcurementParser.ParseDropship(Json);

        Assert.Equal("delivered", drop.Status);
        Assert.Equal("Via Roma 5, 6900 Lugano, CH", drop.RecipientAddress);
        Assert.Equal("DDT-1", drop.DdtNumber);
        Assert.True(drop.BillingAuthorized);
        Assert.Equal("BA-1", drop.BillingAuthorizationReference);
        var movement = Assert.Single(drop.Movements);
        Assert.Equal(120m, movement.QuantityBase);
        Assert.Equal("PZ", movement.BaseUom);
        Assert.Equal(855m, movement.ValuedAmount);
    }

    /// <summary>
    /// The pricing preview answer keeps every amount.
    /// </summary>
    [Fact]
    public void ParsePricing_ReadsTheAmounts()
    {
        const string Json = """
            {"data":{"discount_cascade":["10.00","5.00"],"effective_discount_percent":"14.50","net_unit_price":"85.5000","line_net_amount":"855.00","line_tax_amount":"188.10",
             "line_total_amount":"1043.10","quantity_base":"120.0000","net_price_per_base_unit":"7.1250","line_net_amount_company_currency":"940.50"}}
            """;

        var preview = ProcurementParser.ParsePricing(Json);

        Assert.Equal([10m, 5m], preview.DiscountCascade);
        Assert.Equal(14.5m, preview.EffectiveDiscountPercent);
        Assert.Equal(1043.10m, preview.LineTotalAmount);
        Assert.Equal(7.125m, preview.NetPricePerBaseUnit);
        Assert.Equal(940.5m, preview.LineNetAmountCompanyCurrency);
    }

    /// <summary>
    /// An answer that is not JSON raises, so a bad answer never replaces a good cache.
    /// </summary>
    [Fact]
    public void ParseOrders_NotJson_Throws()
    {
        _ = Assert.ThrowsAny<System.Text.Json.JsonException>(() => ProcurementParser.ParseOrders("<html>"));
    }

    /// <summary>
    /// The actions an order allows follow its status: dispatch from approved or sent, confirmation from sent or confirmed.
    /// </summary>
    /// <param name="status">The wire status.</param>
    /// <param name="dispatch">Whether the order can be dispatched.</param>
    /// <param name="confirm">Whether a confirmation can be recorded.</param>
    [Theory]
    [InlineData("draft", false, false)]
    [InlineData("approved", true, false)]
    [InlineData("sent", true, true)]
    [InlineData("confirmed", false, true)]
    [InlineData("received", false, false)]
    public void Codes_OrderActions_FollowTheStatus(string status, bool dispatch, bool confirm)
    {
        Assert.Equal(dispatch, ProcurementCodes.CanDispatch(status));
        Assert.Equal(confirm, ProcurementCodes.CanConfirm(status));
    }

    /// <summary>
    /// The DDT can be issued once and the delivery recorded until the dropshipment is complete.
    /// </summary>
    [Fact]
    public void Codes_DropshipActions_FollowTheStatus()
    {
        Assert.True(ProcurementCodes.CanIssueDdt("pending"));
        Assert.False(ProcurementCodes.CanIssueDdt("ddt_issued"));
        Assert.True(ProcurementCodes.CanDeliver("ddt_issued"));
        Assert.True(ProcurementCodes.CanDeliver("partially_delivered"));
        Assert.False(ProcurementCodes.CanDeliver("delivered"));
        Assert.False(ProcurementCodes.CanDeliver("pending"));
    }

    /// <summary>
    /// The routes match the endpoints of the web-api.
    /// </summary>
    [Fact]
    public void Routes_MatchTheWebApi()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        Assert.Equal("procurement/purchase-orders/overdue", ProcurementRoutes.Overdue);
        Assert.Equal("procurement/purchase-orders/pricing-preview", ProcurementRoutes.PricingPreview);
        Assert.Equal("procurement/purchase-orders/11111111-1111-1111-1111-111111111111/dispatch", ProcurementRoutes.Dispatch(id));
        Assert.Equal("procurement/purchase-orders/11111111-1111-1111-1111-111111111111/confirmations", ProcurementRoutes.Confirmations(id));
        Assert.Equal("procurement/purchase-orders/11111111-1111-1111-1111-111111111111/reminders", ProcurementRoutes.Reminders(id));
        Assert.Equal("procurement/dropshipments/11111111-1111-1111-1111-111111111111/ddt", ProcurementRoutes.DropshipDdt(id));
        Assert.Equal("procurement/dropshipments/11111111-1111-1111-1111-111111111111/deliveries", ProcurementRoutes.DropshipDeliveries(id));
        Assert.Equal("procurement/purchase-orders?per_page=100&status=confirmed", ProcurementRoutes.OrderList("confirmed"));
        Assert.Equal("procurement/dropshipments?per_page=100", ProcurementRoutes.DropshipList(null));
    }
}
