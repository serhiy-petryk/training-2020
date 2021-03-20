using System;
using System.Windows;
using System.Windows.Media;
using WpfInvestigate.ViewModels;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for ColorControlTests.xaml
    /// </summary>
    public partial class ColorControlTests : Window
    {
        public ColorControlTests()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ColorControl.SaveColor();
            /*var newRes = new ResourceDictionary();
            newRes.Source = new Uri("pack://application:,,,/WpfInvestigate;component/Themes/MwiChild.Wnd10.xaml", UriKind.RelativeOrAbsolute);
            newRes["Mwi.Child.BaseBackgroundColor"] = ColorControl.Color;
            newRes["Mwi.Child.BaseBackgroundBrush"] = new SolidColorBrush(ColorControl.Color);
            // this.Resources.MergedDictionaries.Clear();
            // Resources["Mwi.Child.BaseBackgroundColor"] = ColorControl.Color;

            var rd1 = new ResourceDictionary();
            rd1.Source = new Uri("pack://application:,,,/WpfInvestigate;component/Themes/Mwi.Wnd10.xaml", UriKind.RelativeOrAbsolute);
            // rd1["Mwi.Child.BaseBackgroundColor"] = ColorControl.Color;
            rd1.MergedDictionaries.Clear();
            rd1.MergedDictionaries.Add(newRes);*/
            MwiAppViewModel.Instance.AppColor = ColorControl.Color;
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e) => ColorControl.RestoreColor();

        private void ChangeColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (Brush.Color == Colors.Blue)
                Brush.Color = Colors.Yellow;
            else
                Brush.Color = Colors.Blue;
        }
    }
}
