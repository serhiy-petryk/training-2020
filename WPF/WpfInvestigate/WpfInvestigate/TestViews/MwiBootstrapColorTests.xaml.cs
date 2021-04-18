using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using WpfInvestigate.Common;
using WpfInvestigate.Controls;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for MwiBootstrapColorTests.xaml
    /// </summary>
    public partial class MwiBootstrapColorTests : Window
    {
        public MwiBootstrapColorTests()
        {
            InitializeComponent();
        }

        private void OnAddWindowClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as ButtonBase;
            var userControl = btn.GetVisualParents().OfType<UserControl>().First();
            var container = btn.GetVisualParents().OfType<MwiContainer>().First();

            var mwiChild = new MwiChild { Title = userControl.Tag.ToString() };

            var b1 = new Binding {Path = new PropertyPath("Background"), Source = userControl};
            mwiChild.SetBinding(BackgroundProperty, b1);

            var b2 = new Binding
            {
                Path = new PropertyPath("Background"),
                Source = userControl,
                Converter = ColorHslBrush.Instance,
                ConverterParameter = "+75%"
            };
            mwiChild.SetBinding(ForegroundProperty, b2);

            container.Children.Add(mwiChild);
        }
    }
}
