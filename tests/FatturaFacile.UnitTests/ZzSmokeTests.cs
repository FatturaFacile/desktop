#pragma warning disable
using System.IO;
using System.Windows;
using FatturaFacile.Infrastructure.Services;
using System.Windows.Controls;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using FatturaFacile.Wpf.Views;
using Xunit;

namespace FatturaFacile.UnitTests;

public sealed class ZzSmokeTests
{
    [Fact]
    public void Views_Load()
    {
        var log = new List<string>();
        var a = typeof(System.Windows.Application).GetField("_resourceAssembly", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic); a?.SetValue(null, typeof(FatturaFacile.Wpf.App).Assembly);
        var t = new Thread(() =>
        {
            foreach (var f in new Func<FrameworkElement>[] { () => new ContactsView(), () => new AccountingView(), () => new InvoicingView(), () => new PayrollView(), () => new ProductsView() })
            {
                try
                { var v = f(); log.Add("OK " + v.GetType().Name); }
                catch (Exception ex) { log.Add("FAIL " + ex.GetBaseException().GetType().Name + ": " + ex.GetBaseException().Message + " || " + ex.Message); }
            }

            try
            {
                var app = new FatturaFacile.Wpf.App();
                app.InitializeComponent();
                var ts = new CurrentTenantService();
                var loc = LocalizationService.Instance;
                var mvm = new MainViewModel(null!, null!, ts, loc, new InvoicingViewModel(null!, null!, ts, loc), new AccountingViewModel(null!, ts, loc), new ContactsViewModel(null!, ts, loc), new ProductsViewModel(null!, ts, loc), new PayrollViewModel(null!, ts, loc));
                var w = new MainWindow(mvm);
                log.Add("OK MainWindow; templates=" + w.Resources.Keys.OfType<DataTemplateKey>().Count());
            }
            catch (Exception ex) { log.Add("FAIL shell " + ex.GetBaseException().GetType().Name + ": " + ex.GetBaseException().Message + " || " + ex.Message); }
            var vm = new ContactsViewModel(null!, new CurrentTenantService(), LocalizationService.Instance);
            var tb = new TextBlock { DataContext = vm };
            tb.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(nameof(vm.StatusMessage)));
            vm.StatusMessage = "bound-ok";
            log.Add("BIND " + tb.Text);
            var cmd = new Button { DataContext = vm };
            cmd.SetBinding(Button.CommandProperty, new System.Windows.Data.Binding("LoadContactsCommand"));
            log.Add("CMD " + (cmd.Command != null));
        });
        t.SetApartmentState(ApartmentState.STA);
        t.IsBackground = true;
        t.Start();
        var done = t.Join(60000);
        File.WriteAllLines(Path.Combine(Path.GetTempPath(), "ff-smoke.txt"), log.Append("done=" + done));
        Assert.True(done);
    }
}
