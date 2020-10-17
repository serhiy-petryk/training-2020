using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WpfInvestigate.Controls.Helpers;

namespace WpfInvestigate.Controls.Effects
{
    /// <summary>
    /// </summary>
    public class CornerRadiusEffect
    {
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
            "CornerRadius", typeof(CornerRadius), typeof(CornerRadiusEffect), new UIPropertyMetadata(new CornerRadius(), OnCornerRadiusChanged));
        public static CornerRadius GetCornerRadius(DependencyObject obj) => (CornerRadius)obj.GetValue(CornerRadiusProperty);
        public static void SetCornerRadius(DependencyObject obj, CornerRadius value) => obj.SetValue(CornerRadiusProperty, value);
        private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FrameworkElement element)) return;

            OnObjectUnloaded(element, null);

            element.Unloaded += OnObjectUnloaded;
            element.SizeChanged += UpdateBorders;
            var dpd = DependencyPropertyDescriptor.FromProperty(Border.BorderThicknessProperty, typeof(Border));
            dpd.AddValueChanged(element, UpdateBorders);

            element.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() => UpdateBorders(element, null)));
        }
        private static void OnObjectUnloaded(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            element.Unloaded -= OnObjectUnloaded;
            element.SizeChanged -= UpdateBorders;
            var dpd = DependencyPropertyDescriptor.FromProperty(Border.BorderThicknessProperty, typeof(Border));
            dpd.RemoveValueChanged(element, UpdateBorders);
        }

        private static void UpdateBorders(object sender, EventArgs e)
        {
            if (!(sender is FrameworkElement element) || !element.IsLoaded) return;

            var newCornerRadius = GetCornerRadius(element);
            foreach (var border in ControlHelper.GetMainBorders(element))
            {
                border.CornerRadius = newCornerRadius;
                ControlHelper.ClipToBoundBorder(border);
            }
        }
    }
}
