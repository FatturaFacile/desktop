using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DevExpress.AIIntegration.Wpf.Internal;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Native;

namespace DevExpress.DevAV.Behaviors {
    public class ExpanderBehavior : Behavior<DXExpander> {
        static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(ExpanderBehavior),
                new FrameworkPropertyMetadata(false, (d, e) => ((ExpanderBehavior)d).OnIsExpandedChanged(e)));

        public static readonly DependencyProperty ToggleButtonStyleProperty =
            DependencyProperty.Register(nameof(ToggleButtonStyleProperty), typeof(Style), typeof(ExpanderBehavior), new FrameworkPropertyMetadata(null, (d, e) => ((ExpanderBehavior)d).OnToggleButtonStyleChanged(e)));

        public bool IsExpanded {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        public Style ToggleButtonStyle {
            get => (Style)GetValue(ToggleButtonStyleProperty);
            set => SetValue(ToggleButtonStyleProperty, value);
        }

        ToggleButton button;

        protected override void OnAttached() {
            base.OnAttached();
            if(AssociatedObject.IsLoaded)
                Initialize();
            else
                AssociatedObject.Loaded += OnLoaded;
        }

        protected override void OnDetaching() {
            AssociatedObject.Loaded -= OnLoaded;
            RemoveButton();
            base.OnDetaching();
        }

        void OnIsExpandedChanged(DependencyPropertyChangedEventArgs e) {
            if(AssociatedObject == null)
                return;

            AssociatedObject.IsExpanded = (bool)e.NewValue;
        }

        void OnToggleButtonStyleChanged(DependencyPropertyChangedEventArgs e) {
            if(button == null)
                return;
            button.Style = (Style)e.NewValue;
        }

        void OnLoaded(object sender, RoutedEventArgs e) => Initialize();

        void Initialize() {
            SetCurrentValue(IsExpandedProperty, AssociatedObject.IsExpanded);
            if(button != null) return;

            var window = Window.GetWindow(AssociatedObject) as ThemedWindow;
            if(window == null)
                return;

            var palette = SvgPaletteHelper.GetSvgPalette(window);
            var image = Data.Images.AIImageResources.AISparkleFluentRegularIcon;

            button = new ToggleButton {
                Name = "showChatControl",
                Style = ToggleButtonStyle,
                Content = new DXImage {
                    Source = WpfSvgRenderer.CreateImageSource(image, palette: palette),
                    Width = 16,
                    Height = 16,
                    Margin = new Thickness(1)
                },
            };
            button.Checked += OnButtonClick;
            button.Unchecked += OnButtonClick;
            window.HeaderItems.Add(button);
        }
        void OnButtonClick(object sender, RoutedEventArgs e) {
            AssociatedObject.IsExpanded = button.IsChecked == true;
            SetCurrentValue(IsExpandedProperty, AssociatedObject.IsExpanded);
        }

        void RemoveButton() {
            var window = Window.GetWindow(AssociatedObject) as ThemedWindow;
            if(window != null && button != null && window.HeaderItems.Contains(button))
                window.HeaderItems.Remove(button);
            if(button != null) {
                button.Checked -= OnButtonClick;
                button.Unchecked -= OnButtonClick;
            }
            button = null;
        }
    }
}
