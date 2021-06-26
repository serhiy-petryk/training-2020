using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using WpfSpLib.Common;
using WpfSpLib.Controls;

namespace WpfSpLibDemo.TestViews
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

        public async Task RunTest(int numberOfTestSteps)
        {
            var userControl = this.GetVisualChildren().OfType<UserControl>().FirstOrDefault();
            for (var k = 0; k < numberOfTestSteps; k++)
                await StepOfTest(userControl, k);
        }

        private async void OnRunTestClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as ButtonBase;
            var userControl = btn.GetVisualParents().OfType<UserControl>().First();
            for (var k = 0; k < 5; k++)
                await StepOfTest(userControl, k);
        }

        private async Task StepOfTest(UserControl userControl, int step)
        {
            var container = userControl.GetVisualChildren().OfType<MwiContainer>().First();

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

            await Task.Delay(300);
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var a11 = GC.GetTotalMemory(true);

            mwiChild.Close(null);

            await Task.Delay(300);
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var a12 = GC.GetTotalMemory(true);

            Debug.Print($"Test{step}: {a12:N0}");
        }

        private void OnTestClick(object sender, RoutedEventArgs e)
        {
        }
    }
}
