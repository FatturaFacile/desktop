using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

using FatturaFacile.Wpf.Helpers;

using Syncfusion.UI.Xaml.Chat;

namespace FatturaFacile.Wpf.ViewModels
{
    public class SmartDataGridViewModel : Observable
    {
        public ObservableCollection<OrderInfo> OrderInfoCollection { get; set; }

        private Author currentUser;

        public Author CurrentUser
        {
            get
            {
                return this.currentUser;
            }
            set
            {
                this.currentUser = value;
                RaisePropertyChanged("CurrentUser");
            }
        }

        public ObservableCollection<string> AISuggestions { get; } = new ObservableCollection<string>
        {
            "Which orders have a payment status of Not Paid?",
            "What are the top 10 orders with the highest freight cost?",
            "Which customers have placed the most orders?",
            "What are the orders shipped to Brazil?",
            "What is the total quantity of products ordered across all orders?",
        };

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public SmartDataGridViewModel()
        {
            this.CurrentUser = new Author { Name = "John" };
            OrderInfoCollection = new ObservableCollection<OrderInfo>();
            GenerateOrders();
        }

        private Random random = new Random();
        private static readonly string[] Names = new[]
        {
            "Emma", "Olivia", "Charlotte", "Amelia", "Sophia",
            "William", "Isabella", "Emily", "George", "Grace",
            "Lily","Ava","Chloe","Ella","Mia","James","Sophie",
            "Isla","Henry","Lucy"
        };

        private static readonly string[] Countries = new[]
        {
            "Spain","Brazil","Switzerland","France","Germany","UK","Canada","Mexico","USA","Italy"
        };

        public string[] ElectronicsProducts = new string[]
        {
            "Keyboard", "Mouse", "Trackpad", "Stylus", "Scanner", "Webcam", "Microphone", "Monitor", "Speakers", "Headphones", "Printers", "Projectors", "External Drive", "UPS"
        };

        private static readonly string[] Cities = new[]
        {
            "Madrid","Rio","Bern","Paris","Berlin","London","Toronto","Mexico D.F.","Seattle","Rome"
        };

        private static readonly string[] PaymentStatuses = new[] { "Paid", "Not Paid" };

        private void GenerateOrders()
        {
            var rnd = new Random(42);
            int baseOrderId = 20251001;

            for (int i = 0; i < 100; i++)
            {
                int orderId = baseOrderId + i;
                var name = Names[i % Names.Length];
                string productName = ElectronicsProducts[random.Next(ElectronicsProducts.Length)];
                DateTime orderDate = DateTime.Today.AddDays(-rnd.Next(1, 90));

                // Quantity realistic
                int quantity = rnd.Next(1, 20);

                // Freight based on product type
                int freight;
                if (productName == "Mouse" || productName == "Stylus" || productName == "Trackpad")
                    freight = rnd.Next(50, 150);
                else if (productName == "Keyboard" || productName == "Webcam" || productName == "Microphone")
                    freight = rnd.Next(150, 350);
                else
                    freight = rnd.Next(350, 750);

                // Country and city mapping
                var country = Countries[i % Countries.Length];
                var city = Cities[i % Cities.Length];

                // Payment status with realistic ratio
                var status = rnd.NextDouble() < 0.7 ? "Paid" : "Not Paid";

                OrderInfoCollection.Add(new OrderInfo(
                    orderId: orderId,
                    customerName: name,
                    productName: productName,
                    orderDate: orderDate,
                    quantity: quantity,
                    freight: freight,
                    shipCountry: country,
                    shipCity: city,
                    paymentStatus: status));
            }
        }
    }
	public partial class OrderInfo : Observable
    {
        private int _orderID;
        private string _customerName = string.Empty;
        private string _productName = string.Empty;
        private DateTime _orderDate;
        private int _quantity;
        private double _freight;
        private string _shipCountry = string.Empty;
        private string _shipCity = string.Empty;
        private string _paymentStatus = string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;
            storage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        public int OrderID
        {
            get => _orderID;
            set => SetProperty(ref _orderID, value);
        }

        public string CustomerName
        {
            get => _customerName;
            set => SetProperty(ref _customerName, value);
        }

        public string ProductName
        {
            get => _productName;
            set => SetProperty(ref _productName, value);
        }

        public DateTime OrderDate
        {
            get => _orderDate;
            set => SetProperty(ref _orderDate, value);
        }

        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        public double Freight
        {
            get => _freight;
            set => SetProperty(ref _freight, value);
        }

        public string ShipCountry
        {
            get => _shipCountry;
            set => SetProperty(ref _shipCountry, value);
        }

        public string ShipCity
        {
            get => _shipCity;
            set => SetProperty(ref _shipCity, value);
        }

        public string PaymentStatus
        {
            get => _paymentStatus;
            set => SetProperty(ref _paymentStatus, value);
        }

        public OrderInfo() { }

        public OrderInfo(int orderId,
                         string customerName,
                         string productName,
                         DateTime orderDate,
                         int quantity,
                         double freight,
                         string shipCountry,
                         string shipCity,
                         string paymentStatus)
        {
            _orderID = orderId;
            _customerName = customerName;
            _productName = productName;
            _orderDate = orderDate;
            _quantity = quantity;
            _freight = freight;
            _shipCountry = shipCountry;
            _shipCity = shipCity;
            _paymentStatus = paymentStatus;
        }
    }
}
