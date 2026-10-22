using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.Data.Utils;
using DevExpress.Mvvm.DataAnnotations;

namespace AIDemo.ViewModels {
    [POCOViewModel]
    public class PromptToExpressionModuleViewModel {
        public IList<ShippingRecord> Items { get; } = PromptToExpressionDataHelper.GetData(30);
    }

    public class ShippingRecord {
        public int TrackingID { get; set; }

        [DisplayName("Initial Delivery Date")]
        public DateTime EstimatedDeliveryDate { get; set; }

        [DisplayName("Updated Delivery Date")]
        public DateTime UpdatedDeliveryDate { get; set; }

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

    public static class PromptToExpressionDataHelper {
        public static IList<ShippingRecord> GetData(int count) {
            var records = new List<ShippingRecord>(count);
            var rnd = NonCryptographicRandom.Default;
            DateTime dateTime = DateTime.Today;
            for(int i = 0; i < count; i++) {
                int n = rnd.Next(10);
                double doubleOrderCost = rnd.NextDouble();
                double shippingCost = rnd.NextDouble();
                var record = new ShippingRecord() {
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
}
