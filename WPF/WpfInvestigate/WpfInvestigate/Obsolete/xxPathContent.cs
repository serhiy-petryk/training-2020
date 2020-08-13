using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfInvestigate.Common;

namespace WpfInvestigate.Obsolete
{
    public class xxPathContent
    {
        public static readonly DependencyProperty DataProperty = DependencyProperty.RegisterAttached(
            "Data", typeof(Geometry), typeof(xxPathContent), new UIPropertyMetadata(Geometry.Empty, OnPropertiesChanged));
        public static Geometry GetData(DependencyObject obj) => (Geometry)obj.GetValue(DataProperty);
        public static void SetData(DependencyObject obj, Geometry value) => obj.SetValue(DataProperty, value);

        //=====================================
        private static void OnPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ContentControl cc)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                {
                    var path = new Path { Stretch = Stretch.Uniform, Data = (Geometry)e.NewValue };
                    path.Fill = Tips.GetActualForegroundBrush(cc);
                    cc.Content = new Viewbox { Child = path };
                }));
            }
        }
    }
}
