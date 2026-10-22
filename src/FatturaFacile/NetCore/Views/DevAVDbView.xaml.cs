using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Mvvm;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Ribbon;
using DevExpress.Xpf.Accordion;
using System.Collections;
using DevExpress.AIIntegration.Blazor.Chat;
using DevExpress.DevAV.ViewModels;
#if NET
using DevExpress.AIIntegration;
using DevExpress.AIIntegration.Tools;
using DevExpress.AIIntegration.Wpf.Chat;
using DevExpress.DevAV.AiTools;
using DevExpress.Xpf.Core;
#endif

namespace DevExpress.DevAV.Views {
    public partial class DevAVDbView : UserControl {
        const string SystemMessage =
            @"You are an assistant integrated into an application. Your role is to perform all user-requested UI operations exclusively through the provided tools.
The application consists of modules such as Employees, Customers, Products, and Orders, each containing a grid with relevant data. Tools used to interact with the application are module-scoped, meaning they appear only after navigating to the corresponding module.

Rules:
1. Always use tools to interact with the UI. Do not simulate actions or produce imagined results.
2. If the user’s request refers to any module’s data or operations, you must always identify the target module and call NavigateToModule for that module first, even if the required tool is not currently visible.
3. If a tool requires a tool-target identifier, obtain it only by calling the GetToolTargets tool. Never invent, guess, or construct identifiers; always use one returned by GetToolTargets.
4. Before performing operations on grid data (filtering, adding, editing records, etc.), always switch to the corresponding module first using NavigateToModule.";

        public DevAVDbView() {
            InitializeComponent();
            aiChatControl.LoadMessages(new [] {
                new BlazorChatMessage(Microsoft.Extensions.AI.ChatRole.System, SystemMessage)
            });
            var context = new AIToolsContextBuilder()
                          .WithToolTarget(DataContext, "Application global view model,where you keep all of navigation modules, your view models and grid data sources. Handling navigation.")
                          .WithToolsFromTypes(typeof(DevAvAITools))
                          .Build();
            App.ToolChatContainerDefault.AddAIToolsContext(context);
        }
    }
    public class OutlookChildrenSelector : IChildrenSelector {
        IEnumerable IChildrenSelector.SelectChildren(object item) {
            if(item is DevAVDbModuleDescription)
                return ((DevAVDbModuleDescription)item).FilterTreeViewModel.Categories;
            else if(item is FilterCategory) {
                return ((FilterCategory)item).FilterItems;
            }
            return null;
        }
    }
    public class ObjectsEqualityConverter : System.Windows.Data.IValueConverter {
        public bool Inverse { get; set; }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if(value == null)
                return value;
            var result = string.Equals(value.ToString(), parameter);
            return Inverse ? !result : result;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return (bool)value ? parameter : null;
        }
    }
    public class RibbonStyleSelectorItem : BarSplitButtonItem {
        static RibbonStyle globalRibbonStyle = DevExpress.Xpf.Ribbon.RibbonStyle.Office2010;
        public static RibbonStyle GlobalRibbonStyle {
            get { return globalRibbonStyle; }
            set {
                if(object.Equals(value, globalRibbonStyle))
                    return;
                globalRibbonStyle = value;
                GlobalRibbonStyleChanged(null, EventArgs.Empty);
            }
        }
        public static event EventHandler<EventArgs> GlobalRibbonStyleChanged;
        static RibbonStyleSelectorItem() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonStyleSelectorItem), new FrameworkPropertyMetadata(typeof(RibbonStyleSelectorItem)));
            GlobalRibbonStyleChanged = (s, e) => { };
        }
        ContentControl popupContentControl = new ContentControl();
        public RibbonStyleSelectorItem() {
            ActAsDropDown = true;
            ItemClickBehaviour = PopupItemClickBehaviour.CloseAllPopups;
            PopupControl = new PopupControlContainer() { Content = popupContentControl };
        }
        protected override void OnLoaded(object sender, RoutedEventArgs e) {
            base.OnLoaded(sender, e);
            SelectedRibbonStyle = GlobalRibbonStyle;
            GlobalRibbonStyleChanged += RibbonStyleSelectorItem_GlobalRibbonStyleChanged;
        }
        protected override void OnUnloaded(object sender, RoutedEventArgs e) {
            base.OnUnloaded(sender, e);
            GlobalRibbonStyleChanged -= RibbonStyleSelectorItem_GlobalRibbonStyleChanged;
        }
        void RibbonStyleSelectorItem_GlobalRibbonStyleChanged(object sender, EventArgs e) {
            SelectedRibbonStyle = GlobalRibbonStyle;
        }
        public RibbonStyle SelectedRibbonStyle {
            get { return (RibbonStyle)GetValue(SelectedRibbonStyleProperty); }
            set { SetValue(SelectedRibbonStyleProperty, value); }
        }
        public static readonly DependencyProperty SelectedRibbonStyleProperty =
            DependencyProperty.Register("SelectedRibbonStyle", typeof(RibbonStyle), typeof(RibbonStyleSelectorItem), new PropertyMetadata(DevExpress.Xpf.Ribbon.RibbonStyle.Office2010, OnSelectedRibbonStyleChanged));
        static void OnSelectedRibbonStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            GlobalRibbonStyle = (RibbonStyle)e.NewValue;
        }
        public DataTemplate PopupTemplate {
            get { return (DataTemplate)GetValue(PopupTemplateProperty); }
            set { SetValue(PopupTemplateProperty, value); }
        }
        public static readonly DependencyProperty PopupTemplateProperty =
            DependencyProperty.Register("PopupTemplate", typeof(DataTemplate), typeof(RibbonStyleSelectorItem), new PropertyMetadata(null, OnPopupTemplatePropertyChanged));
        static void OnPopupTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((RibbonStyleSelectorItem)d).popupContentControl.ContentTemplate = (DataTemplate)e.NewValue;
            ((RibbonStyleSelectorItem)d).popupContentControl.Content = new { Selector = d };
        }
    }
}
