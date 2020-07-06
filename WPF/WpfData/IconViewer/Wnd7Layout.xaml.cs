using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using IconViewer.Utils;

namespace IconViewer
{
    /// <summary>
    /// Interaction logic for Wnd7Layout.xaml
    /// </summary>
    public partial class Wnd7Layout : Window
    {
        public RelayCommand StartCommand { get; }
        public RelayCommand CloseCommand { get; }
        public RelayCommand StopCommand { get; }

        public Wnd7Layout()
        {
            InitializeComponent();
            DataContext = this;
            StartCommand = new RelayCommand((p) => Cmd("Start"));
            CloseCommand = new RelayCommand((p) => Cmd("Close"));
            StopCommand = new RelayCommand((p) => Cmd("Stop"));
        }

        private void Cmd(object p)
        {
            MessageBox.Show($"Main {p}");
        }

        private void Thumb_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            MessageBox.Show("Start!");
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Click");
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("DoubleClick");
        }
    }

    public class StringToGeometryConverter : IValueConverter
    {
        public static StringToGeometryConverter Instance = new StringToGeometryConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Geometry.Parse((string)value);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }


}
