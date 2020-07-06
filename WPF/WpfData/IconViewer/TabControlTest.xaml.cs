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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IconViewer
{
    /// <summary>
    /// Interaction logic for TabControlTest.xaml
    /// </summary>
    public partial class TabControlTest : Window
    {
        public TabControlTest()
        {
            InitializeComponent();
        }

        bool _isDragging = false;
        Point _dragStartingPoint;
        TabItem _draggedTab;

        private void TabItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _dragStartingPoint = e.GetPosition(null);
            _draggedTab = (TabItem)sender;
        }

        private void TabItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging || e.LeftButton != MouseButtonState.Pressed) return;

            Point position = e.GetPosition(null);
            if (Math.Abs(position.X - _dragStartingPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(position.Y - _dragStartingPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                _isDragging = true;
                DragDrop.DoDragDrop(_draggedTab, _draggedTab, DragDropEffects.All);
            }
        }

        private void TabItem_Drop(object sender, DragEventArgs e)
        {
            TabItem targetTab = sender as TabItem;
            if (targetTab == null) return;
            if (!e.Data.GetDataPresent(typeof(TabItem))) return;
            TabItem sourceTab = (TabItem)e.Data.GetData(typeof(TabItem));

            if (targetTab == sourceTab) return;

            int targetIdx = tabControl.Items.IndexOf(targetTab);
            using (Dispatcher.DisableProcessing())
            {
                tabControl.Items.Remove(sourceTab);
                tabControl.Items.Insert(targetIdx, sourceTab);
                tabControl.SelectedItem = sourceTab;
            }

        }


        private void TabItemX_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {

            var item = e.Source as TabItem;

            //'Determine if mouse left button is pressed.If((Not(item) Is Nothing) _

            if (item != null && Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(item, item, DragDropEffects.All);
            }
        }

        private void TabItemX_Drop(object sender, DragEventArgs e)
        {
            var target = e.Source as TabItem;
            var source = e.Data.GetData(typeof(TabItem)) as TabItem;
            if (target!=null && source!=null && !object.Equals(source, target))
            {
                var tab = target.Parent as TabControl;
                var sourceIndex = tab.Items.IndexOf(source);
                var targetIndex = tab.Items.IndexOf(target);
                tab.Items.Remove(source);
                tab.Items.Insert(targetIndex, source);
                tab.Items.Remove(target);
                tab.Items.Insert(sourceIndex, target);
            }
        }
    }

}
