using System;
using System.Windows.Markup;
using DevExpress.DevAV.ViewModels;
using DevExpress.Mvvm.Native;
using Microsoft.Extensions.DependencyInjection;

namespace DevExpress.DevAV {
    public class ServiceLocator {
        public static readonly IServiceProvider Instance;
        static ServiceLocator() {
            var services = new ServiceCollection()
                           .AddSingleton<ProductCollectionViewModel>(x => (ProductCollectionViewModel)ViewModelSourceHelper.Create(typeof(ProductCollectionViewModel)))
                           .AddSingleton<CustomerCollectionViewModel>(x => (CustomerCollectionViewModel)ViewModelSourceHelper.Create(typeof(CustomerCollectionViewModel)))
                           .AddSingleton<OrderCollectionViewModel>(x => (OrderCollectionViewModel)ViewModelSourceHelper.Create(typeof(OrderCollectionViewModel)))
                           .AddSingleton<EmployeeCollectionViewModel>(x => (EmployeeCollectionViewModel)ViewModelSourceHelper.Create(typeof(EmployeeCollectionViewModel)));
            Instance = services.BuildServiceProvider();
        }
    }

    public class ViewModelProviderExtension : MarkupExtension {
        public Type Type { get; set; }
        public ViewModelProviderExtension() { }
        public ViewModelProviderExtension(Type type) {
            Type = type;
        }
        public override object ProvideValue(IServiceProvider serviceProvider) => Type != null ? ServiceLocator.Instance.GetService(Type) : null;
    }
}
