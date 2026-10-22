// <copyright file="AiToolingModuleViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.ComponentModel;
using DevExpress.Data.Utils;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;

namespace FatturaFacile.ViewModels;

/// <summary>
/// ViewModel for demonstrating AI tooling integration with data grids.
/// </summary>
[POCOViewModel]
public class AiToolingModuleViewModel
{
    /// <summary>
    /// Gets the observable collection of synthetic orders displayed in the interactive grid.
    /// </summary>
    public ObservableCollection<Order> Items { get; } = new ObservableCollection<Order>(OrderDataHelper.GetData(10));
}

/// <summary>
/// Demonstration order model for AI grid filtering and sorting tools.
/// </summary>
public class Order : BindableBase
{
    /// <summary>Gets or sets the primary integer order sequence identifier.</summary>
    public int OrderID { get; set; }

    /// <summary>Gets or sets the purchasing customer or company name.</summary>
    public string CustomerName { get; set; }

    /// <summary>Gets or sets the calendar date and timestamp when the order was placed.</summary>
    public DateTime OrderDate { get; set; }

    /// <summary>Gets or sets the total invoiced monetary amount of the order.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Gets or sets the description or name of the ordered line item product.</summary>
    public string ProductName { get; set; }

    /// <summary>Gets or sets the discrete unit count of items purchased.</summary>
    public int Quantity { get; set; }

    /// <summary>Gets or sets the current fulfillment lifecycle status (e.g., Pending, Shipped, Delivered).</summary>
    public string Status { get; set; }

    /// <summary>Gets or sets the destination country or geographical region.</summary>
    public string Country { get; set; }
}

/// <summary>
/// Helper class providing sample order data collections.
/// </summary>
public class OrderDataHelper
{
    private static readonly string[] Customers = new[]
    {
        "Alice", "Bob", "Charlie", "Diana", "Evan", "Fiona", "George", "Helen", "Ivan", "Julia",
        "Karl", "Laura", "Martin", "Nina", "Oscar"
    };

    private static readonly string[] Products = new[]
    {
        "Laptop", "Mouse", "Monitor", "Keyboard", "Webcam", "Headset", "Docking Station", "USB Hub", "External SSD",
        "Graphic Tablet"
    };

    private static readonly string[] Countries = new[]
    {
        "USA", "Canada", "UK", "Germany", "France", "Spain", "Italy", "Netherlands", "Australia", "Japan"
    };

    private static readonly string[] Statuses = new[]
    {
        "Pending", "Processing", "Shipped", "Delivered", "Cancelled", "On Hold"
    };

    private static readonly Dictionary<string, decimal> BasePrices =
        new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            { "Laptop", 950m }, { "Mouse", 20m }, { "Monitor", 180m }, { "Keyboard", 45m },
            { "Webcam", 70m }, { "Headset", 85m }, { "Docking Station", 130m }, { "USB Hub", 25m },
            { "External SSD", 150m }, { "Graphic Tablet", 210m }
        };

    /// <summary>
    /// Generates a randomized list of test orders populated with plausible names, product items, and computed prices.
    /// </summary>
    /// <param name="count">The requested quantity of order items to generate.</param>
    /// <returns>A data-bindable list of synthesized <see cref="Order"/> entities.</returns>
    public static BindingList<Order> GetData(
        int count)
    {
        var list = new BindingList<Order>();
        var rnd = NonCryptographicRandom.Default;

        for (var i = 0; i < count; i++)
        {
            var product = Products[rnd.Next(Products.Length)];
            var quantity = rnd.Next(1, 8);
            var basePrice = BasePrices[product];
            var variance = basePrice * (decimal)(rnd.NextDouble() * 0.2 - 0.1);
            var total = Math.Round((basePrice + variance) * quantity, 2);

            var order = new Order
            {
                OrderID = i + 1,
                CustomerName = Customers[i % Customers.Length],
                ProductName = product,
                Quantity = quantity,
                Country = Countries[rnd.Next(Countries.Length)],
                Status = Statuses[rnd.Next(Statuses.Length)],
                OrderDate = DateTime.Today.AddDays(-rnd.Next(0, 30)),
                TotalAmount = total
            };
            list.Add(order);
        }

        return list;
    }
}
