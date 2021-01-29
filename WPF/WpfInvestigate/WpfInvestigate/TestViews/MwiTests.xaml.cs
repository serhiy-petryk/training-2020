using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfInvestigate.Common;
using WpfInvestigate.Controls;
using WpfInvestigate.Samples;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for MwiTests.xaml
    /// </summary>
    public partial class MwiTests : Window
    {
        public MwiTests()
        {
            InitializeComponent();
        }

        private int cnt = 0;
        private void AddChild_OnClick(object sender, RoutedEventArgs e)
        {
            MwiContainer.Children.Add(new MwiChild
            {
                Title = "Window Using Code",
                Content = $"New MwiChild: {cnt++}",
                Width = 300,
                Height = 200,
                Position = new Point(300, 80)
            });
        }

        private void AddChild2_OnClick(object sender, RoutedEventArgs e)
        {
            MwiContainer.Children.Add(new MwiChild
            {
                Title = "Window Using Code",
                Content = new ResizableSample { Width = double.NaN, Height = double.NaN },
                Width = 300,
                Height = 200,
                Position = new Point(300, 80)
            });
        }

        private void Test_OnClick(object sender, RoutedEventArgs e)
        {
            // foreach (var c in MwiContainer.Children)
            // c.AllowDetach = !c.AllowDetach;
            MwiContainer.Children[0].Focus();
            var a1 = Keyboard.FocusedElement;
        }

        private void OpenWindow_OnClick(object sender, RoutedEventArgs e)
        {
            var wnd = new Window();
            wnd.Show();
        }

        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var wnd = Window.GetWindow((DependencyObject)sender);
            var a1 = wnd.ActualWidth;
            var a2 = wnd.ActualHeight;
        }

        private void OnTestButtonClick(object sender, RoutedEventArgs e)
        {
            var a1 = sender as Button;
            var a2 = Tips.GetVisualParents(a1).OfType<MwiChild>().First();
            var a3 = a2.IsActive;
            Debug.Print($"IsActive {a2._controlId} MwiChild: {a3}");
        }
    }
}
