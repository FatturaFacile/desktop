// <copyright file="FieldServiceParserTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Application.FieldService;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the readers of the Field Service payloads: the board, the work orders with the covered and chargeable lines of the
/// service report, the contracts, the equipment history and the offline bundle.
/// </summary>
public sealed class FieldServiceParserTests
{
    /// <summary>
    /// The board keeps the unassigned queue with its skills and parts, and a lane per technician with its van and its bars.
    /// </summary>
    [Fact]
    public void Board_ReadsQueueLanesAndBlocks()
    {
        var board = FieldServiceBoardParser.Parse(FieldServiceTestData.Board);

        Assert.Equal(new DateTimeOffset(2027, 3, 1, 0, 0, 0, TimeSpan.Zero), board.From);
        var order = Assert.Single(board.Unassigned);
        Assert.Equal("WO-1", order.Number);
        Assert.Equal(["hvac", "electrical"], order.RequiredSkills);
        Assert.Equal(2, order.RequiredParts);
        Assert.Equal(46.0037, order.Latitude);
        Assert.Null(order.Start);

        Assert.Equal(2, board.Lanes.Count);
        var anna = board.Lanes[0];
        Assert.True(anna.VanReady);
        Assert.Equal(5, anna.VanProducts);
        var block = Assert.Single(anna.Blocks);
        Assert.Equal(new DateTimeOffset(2027, 3, 2, 8, 0, 0, TimeSpan.Zero), block.Start);
        Assert.Null(block.Address);

        var luca = board.Lanes[1];
        Assert.False(luca.IsAvailable);
        Assert.False(luca.VanReady);
        Assert.Empty(luca.Blocks);
    }

    /// <summary>
    /// A payload that is not a board is refused, so it can never replace the cache.
    /// </summary>
    [Fact]
    public void Board_OfTheWrongShape_Throws()
    {
        _ = Assert.ThrowsAny<JsonException>(() => FieldServiceBoardParser.Parse("[]"));
        _ = Assert.ThrowsAny<JsonException>(() => FieldServiceBoardParser.Parse("not json"));
    }

    /// <summary>
    /// A work order tells the covered parts from the chargeable ones, and the report carries its billing breakdown.
    /// </summary>
    [Fact]
    public void WorkOrder_ReadsPartsCoverageAndReport()
    {
        var order = FieldServiceWorkOrderParser.ParseDetail(FieldServiceTestData.WorkOrder);

        Assert.Equal("preventive", order.WorkType);
        Assert.Equal("Via Roma 1, Lugano", order.Address);
        Assert.Equal(2, order.Parts.Count);
        Assert.True(order.Parts[0].Covered);
        Assert.Equal(2m, order.Parts[0].Quantity);
        Assert.False(order.Parts[1].Covered);
        Assert.Equal(12.5m, order.Parts[1].UnitPrice);
        Assert.All(order.Parts, part => Assert.False(part.IsPending));

        var report = order.Report;
        Assert.NotNull(report);
        Assert.Equal("SR-2027-0001", report.Number);
        Assert.Equal(2.5m, report.LaborHours);
        Assert.Equal(1520.5m, report.MeterHours);
        Assert.Equal(4000, report.MeterCycles);
        Assert.True(report.SignatureIntact);
        Assert.True(report.HasPdf);
        Assert.Equal("de-CH", report.Locale);
        Assert.Equal(2, report.Checklist.Count);
        Assert.Equal("Leak", report.Checklist[1].Notes);
        Assert.Equal(12.5m, report.Billing.BillableTotal);
        Assert.Equal(250m, report.Billing.CoveredTotal);
        Assert.Equal("CHF", report.Billing.Currency);
        Assert.Equal(["labor", "callout", "part"], report.Billing.Lines.Select(line => line.Kind));
        Assert.Equal(0m, report.Billing.Lines[0].ChargedAmount);
        Assert.Equal(200m, report.Billing.Lines[0].ListAmount);
        Assert.True(report.Billing.Lines[0].Covered);
        Assert.False(report.Billing.Lines[2].Covered);
    }

    /// <summary>
    /// The list of work orders has no parts and no report.
    /// </summary>
    [Fact]
    public void WorkOrderList_HasNoDetail()
    {
        var orders = FieldServiceWorkOrderParser.ParseList(FieldServiceTestData.WorkOrderList);

        Assert.Equal(2, orders.Count);
        Assert.Empty(orders[0].Parts);
        Assert.Null(orders[0].Report);
        Assert.Null(orders[1].Start);
    }

    /// <summary>
    /// A report without a signature seal flag is shown as not intact rather than assumed good.
    /// </summary>
    [Fact]
    public void Report_WithoutSealFlag_IsNotIntact()
    {
        var report = FieldServiceWorkOrderParser.ParseReport("{\"id\":\"r\",\"work_order_id\":\"w\"}");

        Assert.False(report.SignatureIntact);
        Assert.Empty(report.Billing.Lines);
        Assert.Equal(0m, report.LaborHours);
    }

    /// <summary>
    /// A contract keeps its coverage type, its preventive rule, its visits and its call-out fee.
    /// </summary>
    [Fact]
    public void Contracts_ReadCoverageFrequencyAndFees()
    {
        var contracts = FieldServiceContractParser.ParseContracts(FieldServiceTestData.Contracts);

        var gold = contracts[0];
        Assert.Equal(MaintenanceCoverageType.PartsAndLabor, gold.Coverage);
        Assert.Equal(new DateOnly(2027, 1, 1), gold.StartDate);
        Assert.Equal(new DateOnly(2027, 12, 31), gold.EndDate);
        Assert.Equal(4, gold.IncludedVisits);
        Assert.Equal(1, gold.UsedVisits);
        Assert.Equal(50m, gold.CalloutFee);
        Assert.Equal(80m, gold.HourlyRate);
        Assert.Equal([FieldServiceTestData.EquipmentId], gold.EquipmentIds);
        Assert.NotNull(gold.Frequency);
        Assert.Equal(6, gold.Frequency.CalendarEvery);
        Assert.Equal("month", gold.Frequency.CalendarUnit);
        Assert.Equal(500m, gold.Frequency.OperatingHours);
        Assert.Equal(10000, gold.Frequency.Cycles);
        Assert.Equal(10m, gold.Frequency.LeadPercent);
        Assert.Equal(14, gold.Frequency.LeadDays);
    }

    /// <summary>
    /// An unknown coverage type is read as the narrowest one, so a contract never looks broader than the server says.
    /// </summary>
    [Fact]
    public void Contracts_UnknownCoverage_IsLaborOnly()
    {
        var plain = FieldServiceContractParser.ParseContracts(FieldServiceTestData.Contracts)[1];

        Assert.Equal(MaintenanceCoverageType.LaborOnly, plain.Coverage);
        Assert.Null(plain.Frequency);
        Assert.Null(plain.EndDate);
        Assert.Null(plain.IncludedVisits);
        Assert.Null(plain.HourlyRate);
    }

    /// <summary>
    /// The coverage types map to the wire names of the server and back.
    /// </summary>
    /// <param name="coverage">The coverage type.</param>
    /// <param name="wire">The wire name.</param>
    [Theory]
    [InlineData(MaintenanceCoverageType.LaborOnly, "labor_only")]
    [InlineData(MaintenanceCoverageType.PartsAndLabor, "parts_and_labor")]
    [InlineData(MaintenanceCoverageType.PreventiveOnly, "preventive_only")]
    [InlineData(MaintenanceCoverageType.FullComprehensive, "full_comprehensive")]
    public void CoverageWire_RoundTrips(MaintenanceCoverageType coverage, string wire)
    {
        Assert.Equal(wire, MaintenanceCoverageWire.ToWire(coverage));
        Assert.Equal(coverage, MaintenanceCoverageWire.FromWire(wire));
    }

    /// <summary>
    /// The equipment list keeps the meters and the service dates.
    /// </summary>
    [Fact]
    public void Equipment_ReadsMetersAndDates()
    {
        var asset = Assert.Single(FieldServiceContractParser.ParseEquipment(FieldServiceTestData.Equipment));

        Assert.Equal("SN-100", asset.SerialNumber);
        Assert.Equal(1520.5m, asset.OperatingHours);
        Assert.Equal(4000, asset.CycleCount);
        Assert.Equal(new DateOnly(2027, 3, 2), asset.LastServiceDate);
        Assert.Equal(new DateOnly(2027, 9, 2), asset.NextServiceDate);
    }

    /// <summary>
    /// The history keeps the reports, the parts with their coverage and the meter readings.
    /// </summary>
    [Fact]
    public void History_ReadsEntriesPartsAndMeters()
    {
        var history = FieldServiceContractParser.ParseHistory(FieldServiceTestData.History);

        Assert.Equal("SN-100", history.SerialNumber);
        var entry = Assert.Single(history.Entries);
        Assert.Equal("SR-2027-0001", entry.ReportNumber);
        Assert.Equal(1520.5m, entry.MeterHours);
        Assert.Equal(4000, entry.MeterCycles);
        Assert.Equal(2, entry.Parts.Count);
        Assert.True(entry.Parts[0].Covered);
        Assert.False(entry.Parts[1].Covered);
    }

    /// <summary>
    /// The offline bundle folds the customer name and the equipment serial into each work order and counts the van stock.
    /// </summary>
    [Fact]
    public void Bundle_FoldsCustomersAndEquipmentIntoTheWorkOrders()
    {
        var bundle = FieldServiceBundleParser.Parse(FieldServiceTestData.Bundle);

        Assert.Equal("Anna Rossi", bundle.Name);
        Assert.Equal(new DateTimeOffset(2027, 3, 2, 7, 0, 0, TimeSpan.Zero), bundle.ServerTime);
        Assert.Equal(2, bundle.VanStockProducts);
        var item = Assert.Single(bundle.Items);
        Assert.Equal("ACME SA", item.CustomerName);
        Assert.Equal("SN-100", item.EquipmentSerial);
        Assert.Equal(["visual_inspection", "lubrication"], item.Checklist);
    }

    /// <summary>
    /// Numbers sent as JSON numbers or as strings are both read.
    /// </summary>
    [Fact]
    public void Amounts_AcceptNumbersAndStrings()
    {
        var report = FieldServiceWorkOrderParser.ParseReport("{\"labor_hours\":2.5,\"travel_hours\":\"1.25\",\"travel_km\":\"abc\"}");

        Assert.Equal(2.5m, report.LaborHours);
        Assert.Equal(1.25m, report.TravelHours);
        Assert.Equal(0m, report.TravelKm);
    }
}
