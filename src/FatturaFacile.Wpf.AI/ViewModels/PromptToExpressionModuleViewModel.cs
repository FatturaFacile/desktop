// <copyright file="PromptToExpressionModuleViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.Data.Utils;
using DevExpress.Mvvm.DataAnnotations;

namespace FatturaFacile.ViewModels;

/// <summary>
/// ViewModel demonstrating natural language prompt-to-expression generation for data filtering and columns.
/// </summary>
[POCOViewModel]
public class PromptToExpressionModuleViewModel
{
    /// <summary>
    /// Gets the sample collection of shipping records subjected to criteria expressions.
    /// </summary>
    public IList<ShippingRecord> Items { get; } = PromptToExpressionDataHelper.GetData(30);
}

/// <summary>
/// Demonstration shipping record with cost and schedule dates for expression evaluation.
/// </summary>
public class ShippingRecord
{
    /// <summary>Gets or sets unique numerical parcel tracking identifier.</summary>
    public int TrackingID { get; set; }

    /// <summary>Gets or sets baseline expected delivery date.</summary>
    [DisplayName("Initial Delivery Date")] public DateTime EstimatedDeliveryDate { get; set; }

    /// <summary>Gets or sets recalculated or updated carrier delivery date.</summary>
    [DisplayName("Updated Delivery Date")] public DateTime UpdatedDeliveryDate { get; set; }

    /// <summary>Gets or sets the gross monetary purchase cost of goods.</summary>
    [DataType(DataType.Currency)]
    [DisplayName("Order Cost ($)")]
    public float OrderCost { get; set; }

    /// <summary>Gets or sets freight or courier logistics charges.</summary>
    [DataType(DataType.Currency)]
    [DisplayName("Shipping Cost ($)")]
    public float ShippingCost { get; set; }

    /// <summary>Gets or sets promotional deduction or rebate amount applied.</summary>
    [DataType(DataType.Currency)]
    [DisplayName("Discount ($)")]
    public float Discount { get; set; }
}

/// <summary>
/// Helper class providing sample shipping record datasets.
/// </summary>
public static class PromptToExpressionDataHelper
{
    /// <summary>
    /// Synthesizes a collection of randomized shipping records for filter testing.
    /// </summary>
    /// <param name="count">The requested quantity of shipping records.</param>
    /// <returns>A list of populated <see cref="ShippingRecord"/> items.</returns>
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
