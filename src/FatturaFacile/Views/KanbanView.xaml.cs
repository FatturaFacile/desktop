using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Grid;

using FatturaFacile.Model;

namespace FatturaFacile.Views;
/// <summary>
/// Interaction logic for KanbanView.xaml
/// </summary>
public partial class KanbanView : UserControl
{
    public KanbanView()
    {
        InitializeComponent();
    }
    private void OnDragRecordOver(object sender, DevExpress.Xpf.Core.DragRecordOverEventArgs e)
    {
        if (e.IsFromOutside && typeof(KanbanItem).IsAssignableFrom(e.GetRecordType()))
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }
    }
    private void OnFadePreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        HideSidePanel();
    }
    private void OnCardClick(object sender, RoutedEventArgs e)
    {
        FrameworkElement clickedElement = (FrameworkElement)sender;
        expander.DataContext = GetRowFromCard(clickedElement);
        expander.Tag = LayoutTreeHelper.GetVisualParents(clickedElement).OfType<TableView>().First().DataContext;
        DisplaySidePanel();
    }
    private void AddCardClick(object sender, RoutedEventArgs e)
    {
        KanbanItem newItem = new KanbanItem();
        GetKanbanGroupFromElement((FrameworkElement)sender).Items.Add(newItem);
        expander.Tag = ((FrameworkElement)sender).DataContext;
        expander.DataContext = newItem;
        DisplaySidePanel();
    }
    private void OnDeleteButtonClick(object sender, RoutedEventArgs e)
    {
        ((KanbanGroup)expander.Tag).Items.Remove((KanbanItem)expander.DataContext);
        HideSidePanel();
    }
    void DisplaySidePanel()
    {
        expander.IsExpanded = true;
        fadeBorder.Visibility = Visibility.Visible;
    }
    void HideSidePanel()
    {
        expander.IsExpanded = false;
        fadeBorder.Visibility = Visibility.Collapsed;
    }
    object GetRowFromCard(FrameworkElement element) => ((RowData)element.DataContext).Row;
    KanbanGroup GetKanbanGroupFromElement(FrameworkElement element) => (KanbanGroup)element.DataContext;
}

public class PanScrollViewer : ScrollViewer
{
    public string IgnoredElementName
    {
        get { return (string)GetValue(IgnoredElementNameProperty); }
        set { SetValue(IgnoredElementNameProperty, value); }
    }
    public static readonly DependencyProperty IgnoredElementNameProperty = DependencyProperty.Register("IgnoredElementName", typeof(string), typeof(PanScrollViewer));

    private Point _lastMousePosition;
    private bool _isPanning = false;

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        var obj = LayoutTreeHelper.GetVisualParents(e.OriginalSource as DependencyObject)
            .OfType<Border>()
            .FirstOrDefault(x => x.Name == IgnoredElementName);
        if (obj == null)
        {
            _lastMousePosition = e.GetPosition(this);
            _isPanning = true;
            CaptureMouse();
        }
        base.OnMouseDown(e);
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_isPanning)
        {
            Point currentPosition = e.GetPosition(this);
            double dx = _lastMousePosition.X - currentPosition.X;
            double dy = _lastMousePosition.Y - currentPosition.Y;

            ScrollToHorizontalOffset(HorizontalOffset + dx);
            ScrollToVerticalOffset(VerticalOffset + dy);

            _lastMousePosition = currentPosition;
        }

        base.OnMouseMove(e);
    }
    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        _isPanning = false;
        ReleaseMouseCapture();
        base.OnMouseUp(e);
    }
}
