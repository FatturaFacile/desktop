using System.Windows.Controls;

namespace FatturaFacile.Views;
/// <summary>
/// Interaction logic for PropertyGridEditorView.xaml
/// </summary>
public partial class PropertyGridBasicView : UserControl
{
    public PropertyGridBasicView()
    {
        InitializeComponent();
    }

    private void OnInitNewOrderItem(object sender, DevExpress.Xpf.PropertyGrid.InstanceInitializeEventArgs e)
    {
        e.Instance.Quantity = 1;
    }
}
