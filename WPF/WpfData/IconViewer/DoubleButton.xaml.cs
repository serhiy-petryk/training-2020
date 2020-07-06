using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace IconViewer
{
    /// <summary>
    /// Interaction logic for DoubleButton.xaml
    /// </summary>
    public partial class DoubleButton : Window
    {
        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            /*var tab = (WindowTab)((FrameworkElement)e.OriginalSource).DataContext;
            if (tab != null && !tab.IsSelected)
                tab.IsSelected = true;*/
        }

        public DoubleButton()
        {
            InitializeComponent();
        }

        private void XXX_OnLoaded(object sender, RoutedEventArgs e)
        {
            var elem = (FrameworkElement) sender;
            elem.Tag = elem;
        }

        private void ButtonRight_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Right");
        }

        private void ButtonLeft_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Left");
        }

        private void FrameworkElement_OnInitialized(object sender, EventArgs e)
        {
            var elem = (FrameworkElement)sender;
            elem.Tag = elem;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            AA.Width = AA.ActualWidth + 4;
            AA.Height = AA.ActualHeight + 4;
        }
    }

    public class DoubleButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Print($"Converter. Value: {value}");
            var elem = value as Polygon;
            if (elem == null) return "0,0";
            return $"0, {elem.ActualHeight}, {elem.ActualWidth}, 0, {elem.ActualWidth}, {elem.ActualHeight}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new CheckoutException("Not ready");
        }
    }

    public class SquareDoubleButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Print($"SquareDoubleButtonConverter. Value: {value}");
            try
            {
                var templateName = (string)parameter;
                var size = (double)value;
                if (templateName == "RightDownButton")
                    return PointCollection.Parse($"0, {size}, {size}, 0, {size}, {size}");
                if (templateName == "LeftUpButton")
                    return PointCollection.Parse($"0, 0, {size}, 0, 0, {size}");
            }
            catch {}
            return $"0, 0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new CheckoutException("Not ready");
        }
    }

    public class DoubleButtonMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (object.Equals(parameter, "Points"))
            {
                try
                {
                    var width = (double) values[0];
                    var height = (double) values[1];
                    var templateName = (string) values[2];

                    if (templateName == "RightDownButton")
                        return PointCollection.Parse($"0, {height}, {width}, 0, {width}, {height}");
                    else if (templateName == "LeftUpButton")
                    {

                    }
                }
                catch
                {

                }
            }
            return PointCollection.Parse($"0, 24, 24, 0, 24, 24, 0, 24");
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new CheckoutException("Not ready");
        }
    }

}
