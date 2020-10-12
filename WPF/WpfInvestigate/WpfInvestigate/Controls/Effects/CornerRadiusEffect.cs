using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

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
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                var border = Helpers.ControlHelper.GetMainBorders(d as FrameworkElement).FirstOrDefault();
                if (border != null)
                    border.CornerRadius = (CornerRadius) e.NewValue;
            }));
        }
    }
}
