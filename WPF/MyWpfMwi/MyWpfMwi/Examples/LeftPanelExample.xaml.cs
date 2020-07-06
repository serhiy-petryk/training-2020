using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MyWpfMwi.Controls;
using MyWpfMwi.Mwi;

namespace MyWpfMwi.Examples
{
    /// <summary>
    /// Interaction logic for LeftPanelExample.xaml
    /// </summary>
    public partial class LeftPanelExample
    {
        private MwiContainer Container => MwiContainer.GetMwiContainer(this);
        private static int count = 0;

        public LeftPanelExample()
        {
            InitializeComponent();
        }

        DGCore.Menu.RootMenu _rootMenu = new DGCore.Menu.RootMenu(@"config.json");
        private void AddDGView0_OnClick(object sender, RoutedEventArgs e) => AddDGView(0);
        private void AddDGView1_OnClick(object sender, RoutedEventArgs e) => AddDGView(1);
        private void AddDGView2_OnClick(object sender, RoutedEventArgs e) => AddDGView(2);
        private void AddDGView(int option)
        {
            var x1 = (DGCore.Menu.MenuOption)((DGCore.Menu.SubMenu)((DGCore.Menu.SubMenu)_rootMenu.Items[1]).Items[1]).Items[option];
            var dd = x1.GetDataDefiniton();
            // Bind(dd, null, null, null, null);

            var a1 = new DGView();
            a1.Bind(dd, null, null, null, null);

            Container.Children.Add(new MwiChild
            {
                Title = dd._description,
                Content = a1,
                Width = 800,
                Height = 600
            });
            Container.HideLeftPanel();
        }

        private void AddExampleWindow_OnClick(object sender, RoutedEventArgs e)
        {
            Container.Children.Add(new MwiChild
            {
                Title = "Window Using Code",
                Content = new ExampleControl(),
                Width = 514,
                Height = 434,
                Position = new Point(300, 80)
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
            StackPanel sp = new StackPanel { Orientation = Orientation.Vertical };
            sp.Children.Add(new TextBlock { Text = "Window with scroll", Margin = new Thickness(5) });
            sp.Children.Add(new ComboBox { Margin = new Thickness(20), Width = 300 });
            ScrollViewer sv = new ScrollViewer { Content = sp, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto };

            Container.Children.Add(new MwiChild { Content = sv, Title = "Window " + count++ });
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
            var a = Container.Children.FirstOrDefault(w => w.Id == 1);
            if (a != null) 
                a.AllowMaximize = !a.AllowMaximize;
        }

    }
}
