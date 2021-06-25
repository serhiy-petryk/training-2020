using System;
using System.Windows;
using System.Windows.Controls;
using WpfSpLib.Controls;

namespace WpfSpLibDemo.Samples
{
    /// <summary>
    /// Interaction logic for LeftPanelSample.xaml
    /// </summary>
    public partial class MwiLeftPanelSample
    {
        private MwiContainer Container => MwiContainer.GetMwiContainer(this);
        private static int count = 0;

        public MwiLeftPanelSample()
        {
            InitializeComponent();
        }

        private void AddSimplestWindow_OnClick(object sender, RoutedEventArgs e)
        {
            Container.Children.Add(new MwiChild
            {
                Title = "Title",
                Content = "Simplest window"
            });
            Container.HideLeftPanel();
        }

        private void AddExampleWindow_OnClick(object sender, RoutedEventArgs e)
        {
            Container.Children.Add(new MwiChild
            {
                Title = "Window Using Code",
                Content = new MwiExampleControl(),
                Width = 514,
                Height = 434,
                Position = new Point(300, 80),
                StatusBar = new MwiStatusBarSample(),
                CommandBar = new MwiCommandBarSample()
            });
            Container.HideLeftPanel();
        }

        private void AddNormalWindow_OnClick(object sender, RoutedEventArgs e)
        {
            Container.Children.Add(new MwiChild { Content = new Label { Content = "Normal window" }, Title = "Window " + count++ });
            Container.HideLeftPanel();
        }

        private void AddFixedWindow_OnClick(object sender, RoutedEventArgs e)
        {
            Container.Children.Add(new MwiChild { Content = new Label { Content = "Fixed width window" }, Title = "Window " + count++, Resizable = false });
            Container.HideLeftPanel();
        }

        private void AddScrollWindow_OnClick(object sender, RoutedEventArgs e)
        {
            var sp = new StackPanel { Orientation = Orientation.Vertical };
            sp.Children.Add(new TextBlock { Text = "Window with scroll", Margin = new Thickness(5) });
            sp.Children.Add(new ComboBox { Margin = new Thickness(20), Width = 300 });
            var sv = new ScrollViewer { Content = sp, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto };

            Container.Children.Add(new MwiChild { Content = sv, Title = "Window " + count++, Width = 200});
            Container.HideLeftPanel();
        }

        private void Debug_OnClick(object sender, RoutedEventArgs e)
        {
            // clear memory
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            //
            var k = GC.GetTotalMemory(true) / 1000000;
        }

        private void Debug2_OnClick(object sender, RoutedEventArgs e)
        {
            /*var a = Container.Children.FirstOrDefault(w => w.Id == 1);
            if (a != null)
                a.AllowMaximize = !a.AllowMaximize;*/
        }


    }
}
