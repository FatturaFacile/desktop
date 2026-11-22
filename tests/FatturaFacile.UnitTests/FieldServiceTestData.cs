// <copyright file="FieldServiceTestData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Payloads of the Field Service endpoints, as the web-api sends them (decimals are strings, instants are ISO 8601).
/// </summary>
internal static class FieldServiceTestData
{
    /// <summary>The id of the sample work order.</summary>
    public const string OrderId = "aaaaaaaa-0000-0000-0000-000000000001";

    /// <summary>The id of the second sample work order.</summary>
    public const string OtherOrderId = "aaaaaaaa-0000-0000-0000-000000000002";

    /// <summary>The id of the sample technician.</summary>
    public const string TechnicianId = "bbbbbbbb-0000-0000-0000-000000000001";

    /// <summary>The id of the technician without the skills and without a van.</summary>
    public const string JuniorId = "bbbbbbbb-0000-0000-0000-000000000002";

    /// <summary>The id of the sample equipment.</summary>
    public const string EquipmentId = "cccccccc-0000-0000-0000-000000000001";

    /// <summary>The id of the sample contract.</summary>
    public const string ContractId = "dddddddd-0000-0000-0000-000000000001";

    /// <summary>The id of the sample service report.</summary>
    public const string ReportId = "eeeeeeee-0000-0000-0000-000000000001";

    /// <summary>The id of the sample product.</summary>
    public const string ProductId = "ffffffff-0000-0000-0000-000000000001";

    /// <summary>The dispatcher board: one unassigned work order, a skilled technician with a visit and a junior without a van.</summary>
    public static readonly string Board = Json(
        "{'window':{'from':'2027-03-01T00:00:00+00:00','to':'2027-03-08T00:00:00+00:00'}," +
        "'unassigned':[{'work_order_id':'" + OrderId + "','work_order_number':'WO-1','title':'Chiller service','work_type':'preventive','priority':'urgent','status':'unscheduled'," +
        "'scheduled_start':null,'scheduled_end':null,'site':{'address':'Via Roma 1, Lugano','latitude':46.0037,'longitude':8.9511}," +
        "'required_skills':['hvac','electrical'],'required_parts':[{'product_id':'p1','quantity':1},{'product_id':'p2','quantity':2}]}]," +
        "'lanes':[{'technician_id':'" + TechnicianId + "','name':'Anna Rossi','is_available':true,'skills':['hvac','electrical']," +
        "'van':{'warehouse_id':'w1','ready':true,'distinct_products_in_stock':5}," +
        "'blocks':[{'work_order_id':'" + OtherOrderId + "','work_order_number':'WO-2','title':'Pump repair','work_type':'corrective','priority':'normal','status':'dispatched'," +
        "'scheduled_start':'2027-03-02T08:00:00+00:00','scheduled_end':'2027-03-02T12:00:00+00:00','site':{'address':null,'latitude':null,'longitude':null}}]}," +
        "{'technician_id':'" + JuniorId + "','name':'Luca Verdi','is_available':false,'skills':['hvac'],'van':{'warehouse_id':null,'ready':false,'distinct_products_in_stock':0},'blocks':[]}]}");

    /// <summary>A signed service report with its billing breakdown.</summary>
    public static readonly string Report = Json(
        "{'id':'" + ReportId + "','work_order_id':'" + OrderId + "','report_number':'SR-2027-0001','work_performed_summary':'Filter replaced'," +
        "'checklist':[{'item':'visual_inspection','result':'pass','notes':null},{'item':'lubrication','result':'fail','notes':'Leak'}]," +
        "'labor_hours':'2.50','travel_hours':'0.50','travel_km':'30.0','apply_callout_fee':true,'meter_reading_hours':'1520.5','meter_reading_cycles':4000," +
        "'customer_signer_name':'Mario Bianchi','customer_signer_role':'Facility manager','signature_svg':'<svg/>','signature_sha256_hash':'abc'," +
        "'signature_intact':true,'signed_at':'2027-03-02T11:30:00+00:00','locale':'de-CH','has_pdf':true,'invoice_id':null," +
        "'billing':{'lines':[" +
        "{'kind':'labor','description':'labor','quantity':'2.50','unit_price':'80.00','list_amount':'200.00','charged_amount':'0.00','covered':true,'product_id':null}," +
        "{'kind':'callout','description':'callout','quantity':'1','unit_price':'50.00','list_amount':'50.00','charged_amount':'0.00','covered':true,'product_id':null}," +
        "{'kind':'part','description':'Gasket','quantity':'1.000','unit_price':'12.50','list_amount':'12.50','charged_amount':'12.50','covered':false,'product_id':'p2'}]," +
        "'billable_total':'12.50','covered_total':'250.00','currency':'CHF'}}");

    /// <summary>A work order with a covered part, a chargeable part and a signed report.</summary>
    public static readonly string WorkOrder = Json(
        "{'id':'" + OrderId + "','work_order_number':'WO-1','work_order_type':'preventive','status':'completed','priority':'high','title':'Chiller service'," +
        "'problem_description':'Noise','customer_id':'cust-1','equipment_passport_id':'" + EquipmentId + "','service_contract_id':'" + ContractId + "'," +
        "'assigned_technician_id':'" + TechnicianId + "','required_skills':['hvac'],'required_parts':[],'scheduled_start':'2027-03-02T08:00:00+00:00'," +
        "'scheduled_end':'2027-03-02T12:00:00+00:00','site':{'address':'Via Roma 1, Lugano','lat':46.0,'lng':8.9},'client_uuid':null,'invoice_id':null," +
        "'parts':[{'id':'pc1','product_id':'" + ProductId + "','description':'Filter','quantity':'2.000','unit_price':'0.00','covered':true,'warehouse_id':'w1','stock_movement_id':'m1'}," +
        "{'id':'pc2','product_id':'p2','description':'Gasket','quantity':'1.000','unit_price':'12.50','covered':false,'warehouse_id':'w1','stock_movement_id':'m2'}]," +
        "'service_report':" + Report + "}");

    /// <summary>The list of work orders (the detail fields are absent).</summary>
    public static readonly string WorkOrderList = Json(
        "[{'id':'" + OrderId + "','work_order_number':'WO-1','work_order_type':'preventive','status':'dispatched','priority':'high','title':'Chiller service'," +
        "'scheduled_start':'2027-03-02T08:00:00+00:00','scheduled_end':'2027-03-02T12:00:00+00:00','required_skills':[],'required_parts':[],'site':{}}," +
        "{'id':'" + OtherOrderId + "','work_order_number':'WO-2','work_order_type':'corrective','status':'unscheduled','priority':'normal','title':'Pump repair'," +
        "'scheduled_start':null,'scheduled_end':null,'required_skills':[],'required_parts':[],'site':{}}]");

    /// <summary>The maintenance contracts.</summary>
    public static readonly string Contracts = Json(
        "[{'id':'" + ContractId + "','customer_id':'cust-1','contract_number':'MC-001','title':'Chiller plan','start_date':'2027-01-01','end_date':'2027-12-31'," +
        "'status':'active','coverage_type':'parts_and_labor','preventive_frequency_rule':{'calendar':{'every':6,'unit':'month'},'meter':{'operating_hours':500,'cycles':10000,'lead_percent':10},'lead_days':14}," +
        "'included_visits_per_year':4,'used_visits_current_year':1,'callout_fee_amount':'50.00','hourly_labor_rate':'80.00','recurring_billing_subscription_id':null," +
        "'currency':'CHF','notes':'Gold','equipment_passport_ids':['" + EquipmentId + "']}," +
        "{'id':'dddddddd-0000-0000-0000-000000000002','customer_id':'cust-2','contract_number':'MC-002','title':'Labor only','start_date':'2027-02-01','end_date':null," +
        "'status':'draft','coverage_type':'mystery','preventive_frequency_rule':null,'included_visits_per_year':null,'used_visits_current_year':0,'callout_fee_amount':'0.00'," +
        "'hourly_labor_rate':null,'currency':'EUR','notes':null,'equipment_passport_ids':[]}]");

    /// <summary>The customer equipment.</summary>
    public static readonly string Equipment = Json(
        "[{'id':'" + EquipmentId + "','serial_number':'SN-100','product_id':null,'customer_id':'cust-1','service_contract_id':'" + ContractId + "'," +
        "'site_address':'Via Roma 1','site_latitude':null,'site_longitude':null,'current_operating_hours':'1520.5','current_cycle_count':4000," +
        "'last_service_date':'2027-03-02','next_scheduled_service_date':'2027-09-02'}]");

    /// <summary>The service history of the sample equipment.</summary>
    public static readonly string History = Json(
        "{'equipment':{'id':'" + EquipmentId + "','serial_number':'SN-100','current_operating_hours':'1520.5','current_cycle_count':4000,'last_service_date':'2027-03-02','next_scheduled_service_date':'2027-09-02'}," +
        "'history':[{'report_id':'" + ReportId + "','report_number':'SR-2027-0001','work_order_id':'" + OrderId + "','work_type':'preventive','signed_at':'2027-03-02T11:30:00+00:00'," +
        "'summary':'Filter replaced','labor_hours':'2.50','meter_reading_hours':'1520.5','meter_reading_cycles':4000," +
        "'parts':[{'product_id':'" + ProductId + "','quantity':'2.000','covered':true},{'product_id':'p2','quantity':'1.000','covered':false}]}]}");

    /// <summary>The offline bundle of the sample technician.</summary>
    public static readonly string Bundle = Json(
        "{'server_time':'2027-03-02T07:00:00+00:00','technician':{'id':'" + TechnicianId + "','name':'Anna Rossi','skills':['hvac'],'van_warehouse_id':'w1'}," +
        "'open_work_order_ids':['" + OrderId + "'],'work_orders':[{'id':'" + OrderId + "','work_order_number':'WO-1','work_type':'preventive','priority':'high','status':'dispatched'," +
        "'title':'Chiller service','problem_description':null,'customer_id':'cust-1','equipment_passport_id':'" + EquipmentId + "','service_contract_id':null," +
        "'scheduled_start':'2027-03-02T08:00:00+00:00','scheduled_end':'2027-03-02T12:00:00+00:00','site':{'address':'Via Roma 1'},'required_skills':['hvac']," +
        "'checklist_template':[{'item':'visual_inspection'},{'item':'lubrication'}],'updated_at':'2027-03-01T10:00:00+00:00'}]," +
        "'customers':[{'id':'cust-1','name':'ACME SA','email':null,'phone':null}]," +
        "'equipment':[{'id':'" + EquipmentId + "','serial_number':'SN-100','product_id':null,'site_address':null,'site_latitude':null,'site_longitude':null,'current_operating_hours':'1520.5','current_cycle_count':4000,'last_service_date':null,'history':[]}]," +
        "'van_stock':[{'product_id':'p1','available':'3'},{'product_id':'p2','available':'1'}]}");

    /// <summary>The body of a successful preventive run.</summary>
    public static readonly string PreventiveRun = Json("{'data':{'evaluated_assets':7,'created':['w1','w2'],'skipped_existing':3}}");

    /// <summary>The 422 answer of a blocked dispatch.</summary>
    public static readonly string DispatchBlocked = Json(
        "{'message':'The given data was invalid.','errors':{'schedule':['Overlaps WO-2'],'skills':['Missing skills: electrical'],'van_stock':['No van']}}");

    /// <summary>Gets the sample work order as a <see cref="Guid"/>.</summary>
    public static Guid OrderGuid { get; } = new(0xAAAAAAAA, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);

    /// <summary>Gets the sample product as a <see cref="Guid"/>.</summary>
    public static Guid ProductGuid { get; } = new(0xFFFFFFFF, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);

    /// <summary>Wraps a payload in the `data` envelope of the web-api.</summary>
    /// <param name="payload">The payload.</param>
    /// <returns>The answer body.</returns>
    public static string Envelope(string payload) => "{\"data\":" + payload + "}";

    /// <summary>Builds a mobile-sync answer with one result per client identifier.</summary>
    /// <param name="results">The (client identifier, status) pairs; a status of `rejected` carries a field error.</param>
    /// <returns>The answer body.</returns>
    public static string SyncAnswer(params (string ClientUuid, string Status)[] results) => Envelope(
        "{'results':[" + string.Join(',', results.Select(r => string.Equals(r.Status, "rejected", StringComparison.Ordinal)
            ? "{'client_uuid':'" + r.ClientUuid + "','type':'part_consumption','status':'rejected','errors':{'quantity':['Not enough stock']}}"
            : "{'client_uuid':'" + r.ClientUuid + "','type':'part_consumption','status':'" + r.Status + "'}")) + "],'server_time':'2027-03-02T10:00:00+00:00'}").Replace('\'', '"');

    /// <summary>Builds a pending outbox entry.</summary>
    /// <param name="operation">The operation.</param>
    /// <param name="workOrder">The work order.</param>
    /// <param name="payload">The payload JSON.</param>
    /// <param name="at">When the technician acted.</param>
    /// <returns>The entry.</returns>
    public static FieldServiceOutboxEntry Entry(FieldServiceOperationType operation, string workOrder, string payload, DateTime at) => new()
    {
        Operation = operation,
        WorkOrderId = Guid.Parse(workOrder),
        PayloadJson = payload,
        OccurredAtUtc = at,
        NextAttemptAt = at,
    };

    /// <summary>Builds a part input for the sample work order.</summary>
    /// <param name="quantity">The quantity.</param>
    /// <returns>The input.</returns>
    public static FieldServicePartInput Part(decimal quantity = 2m) =>
        new(OrderGuid, ProductGuid, "Filter", quantity, 12.5m, LotNumber: null);

    /// <summary>Builds a valid report input for the sample work order.</summary>
    /// <returns>The input.</returns>
    public static FieldServiceReportInput ReportInput() => new(
        OrderGuid,
        "Filter replaced",
        [new FieldServiceChecklistItem("visual_inspection", "pass", Notes: null)],
        LaborHours: 2.5m,
        TravelHours: 0.5m,
        TravelKm: 30m,
        ApplyCalloutFee: true,
        MeterHours: 1520.5m,
        MeterCycles: 4000,
        SignerName: "Mario Bianchi",
        SignerRole: "Facility manager",
        SignatureSvg: "<svg xmlns=\"http://www.w3.org/2000/svg\"><path d=\"M0 0\"/></svg>",
        Locale: "de-CH");

    private static string Json(string text) => text.Replace('\'', '"');
}
