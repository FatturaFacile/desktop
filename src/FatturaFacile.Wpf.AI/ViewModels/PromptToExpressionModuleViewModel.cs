// <copyright file="PromptToExpressionModuleViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.Data.Utils;
using DevExpress.Mvvm.DataAnnotations;

namespace FatturaFacile.ViewModels;

[POCOViewModel]
/// <summary>
/// ViewModel demonstrating natural language prompt-to-expression generation for data filtering and columns.
/// </summary>
public class PromptToExpressionModuleViewModel
{
    public IList<ShippingRecord> Items { get; } = PromptToExpressionDataHelper.GetData(30);
}

/// <summary>
/// Demonstration shipping record with cost and schedule dates for expression evaluation.
/// </summary>
public class ShippingRecord
{
    public int TrackingID { get; set; }

    [DisplayName("Initial Delivery Date")] public DateTime EstimatedDeliveryDate { get; set; }

    [DisplayName("Updated Delivery Date")] public DateTime UpdatedDeliveryDate { get; set; }

    [DataType(DataType.Currency)]
    [DisplayName("Order Cost ($)")]
    public float OrderCost { get; set; }

    [DataType(DataType.Currency)]
    [DisplayName("Shipping Cost ($)")]
    public float ShippingCost { get; set; }

    [DataType(DataType.Currency)]
    [DisplayName("Discount ($)")]
    public float Discount { get; set; }
}

/// <summary>
/// Helper class providing sample shipping record datasets.
/// </summary>
public static class PromptToExpressionDataHelper
{
    public static IList<ShippingRecord> GetData(
        int count)
    {
        var records = new List<ShippingRecord>(count);
        var rnd = NonCryptographicRandom.Default;
        var dateTime = DateTime.Today;
        for (var i = 0; i < count; i++)
        {
            var n = rnd.Next(10);
            var doubleOrderCost = rnd.NextDouble();
            var shippingCost = rnd.NextDouble();
            var record = new ShippingRecord()
            {
                TrackingID = i + 1000,
                EstimatedDeliveryDate = dateTime.AddDays(-(n % 3)),
                UpdatedDeliveryDate = dateTime.AddDays(n % 9 - 2),
                OrderCost = (float)(doubleOrderCost * 500 + 50),
                ShippingCost = (float)(shippingCost * 50 + 10),
                Discount = (i % 3 == 0) ? 10 : 0
            };
            records.Add(record);
        }

        return records;
    }
}
