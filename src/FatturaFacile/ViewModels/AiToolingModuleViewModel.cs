using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DevExpress.Data.Utils;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;

namespace AIDemo.ViewModels {
    [POCOViewModel]
    public class AiToolingModuleViewModel {
        public ObservableCollection<Order> Items { get; } = new ObservableCollection<Order>(OrderDataHelper.GetData(10));
    }

    public class Order : BindableBase {
        public int OrderID { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public string Country { get; set; }
    }

    public class OrderDataHelper {
      static readonly string[] Customers = new[] {
        "Alice","Bob","Charlie","Diana","Evan","Fiona","George","Helen","Ivan","Julia",
        "Karl","Laura","Martin","Nina","Oscar"
    };

    static readonly string[] Products = new[] {
        "Laptop","Mouse","Monitor","Keyboard","Webcam","Headset","Docking Station","USB Hub","External SSD","Graphic Tablet"
    };

    static readonly string[] Countries = new[] {
        "USA","Canada","UK","Germany","France","Spain","Italy","Netherlands","Australia","Japan"
    };

    static readonly string[] Statuses = new[] {
        "Pending","Processing","Shipped","Delivered","Cancelled","On Hold"
    };

    static readonly Dictionary<string, decimal> BasePrices = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) {
        { "Laptop", 950m }, { "Mouse", 20m }, { "Monitor", 180m }, { "Keyboard", 45m },
        { "Webcam", 70m }, { "Headset", 85m }, { "Docking Station", 130m }, { "USB Hub", 25m },
        { "External SSD", 150m }, { "Graphic Tablet", 210m }
    };

    public static BindingList<Order> GetData(int count) {
        var list = new BindingList<Order>();
        var rnd = NonCryptographicRandom.Default;

        for (int i = 0; i < count; i++) {
            var product = Products[rnd.Next(Products.Length)];
            int quantity = rnd.Next(1, 8);
            decimal basePrice = BasePrices[product];
            decimal variance = basePrice * (decimal)(rnd.NextDouble() * 0.2 - 0.1);
            decimal total = Math.Round((basePrice + variance) * quantity, 2);

            var order = new Order {
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
}
