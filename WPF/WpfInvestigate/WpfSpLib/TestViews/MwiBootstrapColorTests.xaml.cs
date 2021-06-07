using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using WpfSpLib.Common;
using WpfSpLib.Controls;
using WpfSpLib.Helpers;

namespace WpfSpLib.TestViews
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

        private async void OnRunTestClick(object sender, RoutedEventArgs e)
        {
            Debug.Print($"Run Tests"); // !!! Debug.Print decrease memory leak on ~25%
            for (var k = 0; k < 5; k++)
                await StepOfTest(sender, k);
        }

        private async Task StepOfTest(object sender, int step)
        {
            var btn = sender as ButtonBase;
            var userControl = btn.GetVisualParents().OfType<UserControl>().First();
            var container = btn.GetVisualParents().OfType<MwiContainer>().First();

            var mwiChild = new MwiChild { Title = userControl.Tag.ToString() };
            var b1 = new Binding { Path = new PropertyPath("Background"), Source = userControl };
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

            await Task.Delay(1000);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var a11 = GC.GetTotalMemory(true);

            mwiChild.Close(null);

            await Task.Delay(1000);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var a12 = GC.GetTotalMemory(true);

            Debug.Print($"Test{step}: {a11:N0}, {a12:N0}");
        }

        public void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // need to clear bindings to prevent an error in AutomaticUnloading.ClearElement
            if (this.IsElementDisposing())
                BindingOperations.ClearAllBindings(MainGrid.LayoutTransform);

            this.AutomaticUnloading(OnUnloaded);
        }

        private void OnTestClick(object sender, RoutedEventArgs e)
        {
        }
    }
}
