using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace IconViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            /*if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
            
            SizeToContent = SizeToContent.Manual;*/
            var window = new Window {WindowState = WindowState.Maximized};
            // window.Visibility = Visibility.Hidden;
            window.Show();
            var a1 = window.ActualHeight;
            var a2 = window.ActualWidth;
            var a3 = window.Top;
            var a4 = window.Left;
            window.Close();

            var maxWindow = new MaxTest {Top = a3, Left = a4, Width = a2, Height = a1};
            // maxWindow.BorderThickness = new Thickness(2, 0, 2, 0);
            maxWindow.BorderBrush = new SolidColorBrush(Colors.Red);
            //maxWindow.Visibility = Visibility.Hidden;
            maxWindow.Show();
            maxWindow.Left += (a2 - maxWindow.ActualWidth) / 2;
            //maxWindow.Visibility = Visibility.Visible;
            // maxWindow.Show();


            var a11 = maxWindow.ActualHeight;
            var a21 = maxWindow.ActualWidth;
            // maxWindow.Width +=  a2  - maxWindow.ActualWidth;
            // maxWindow.Width = 2222;
            // MessageBox.Show($"Width:{a2} . Height: {a1}");
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var a1 = Application.Current.Resources;
            var a2 = XamlWriter.Save(a1);
        }
    }
}
