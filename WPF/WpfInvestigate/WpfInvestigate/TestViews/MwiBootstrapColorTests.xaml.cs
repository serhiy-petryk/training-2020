using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using WpfInvestigate.Common;
using WpfInvestigate.Controls;
using WpfInvestigate.Helpers;

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

        private async void OnRunTestClick(object sender, RoutedEventArgs e)
        {
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

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (this.IsElementDisposing())
            {
                // BindingOperations.ClearAllBindings(this);
                //foreach(var child in Tips.GetVisualChildren(this))
                //  BindingOperations.ClearAllBindings(this);
                // BindingOperations.ClearAllBindings(MainGrid.LayoutTransform);
                /*var elements = (new[] { this }).Union(this.GetVisualChildren()).ToArray();
                foreach (var element in elements)
                {
                EventHelper.RemoveWpfEventHandlers(element);
                    Events.RemoveAllEventSubsriptions(element);
                }*/
                // Debug.Print($"MwiBootstrapColorTests. Unloaded");
                Unloaded -= OnUnloaded;

                Debug.Print($"Test. OnUnloaded. {CleanerHelper.ClearCount}");

                var elements = (new[] { this }).Union(this.GetVisualChildren()).ToArray();
                foreach (var element in elements)
                {
                    EventHelper.RemoveWpfEventHandlers(element);
                    Events.RemoveAllEventSubsriptions(element);
                }

                this.CleanDependencyObject();

                Debug.Print($"Clear count: {CleanerHelper.ClearCount}");
                // CleanerHelper.ClearCount = 0;
            }
        }

        private void OnTestClick(object sender, RoutedEventArgs e)
        {
            var elements = (new[] { this }).Union(this.GetVisualChildren()).ToArray();
            foreach (var element in elements)
            {
                EventHelper.RemoveWpfEventHandlers(element);
                Events.RemoveAllEventSubsriptions(element);
            }
        }
    }
}
