using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ItemsControlDragDrop.Code;

namespace ItemsControlDragDrop
{
    /// <summary>
    /// Interaction logic for MyClearVersion.xaml
    /// </summary>
    public partial class MyClearVersion : Window
    {
        public ObservableCollection<string> MyData =>
            new ObservableCollection<string>(new string[]{
                "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten",
                "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen", "twenty"
            });

        public MyClearVersion()
        {
            InitializeComponent();
            DataContext = this;

            // this.listView.ItemsSource = MyData;
            // this.listView2.ItemsSource = new ObservableCollection<string>();
            this.view1.ItemsSource = MyTask.CreateTasks();
            this.view2.ItemsSource = new ObservableCollection<MyTask>();
        }

        private void ItemsList_DragOver(object sender, DragEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            var sv = FindVisualChild<ScrollViewer>(dataGrid);

            double tolerance = 30;
            double verticalPos = e.GetPosition(dataGrid).Y;
            double offset = 1;

            //Debug.Print($"Drag: {verticalPos}");
            if (verticalPos < tolerance) // Top of visible list?
            {
                //Debug.Print($"ScrollUp");
                sv.ScrollToVerticalOffset(sv.VerticalOffset - offset); //Scroll up.
            }
            else if (verticalPos > dataGrid.ActualHeight - tolerance) //Bottom of visible list?
            {
                //Debug.Print($"ScrollDown");
                sv.ScrollToVerticalOffset(sv.VerticalOffset + offset); //Scroll down.    
            }
        }

        private DraggableAdorner myAdornment;
        private bool _isDragging;
        private void Item_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                if (e.LeftButton != MouseButtonState.Pressed)
                    _isDragging = false;
                return;
            }
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            Task.Factory.StartNew(new Action(() =>
            {
                _isDragging = true;
                Thread.Sleep(100);
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        var dataObject = new DataObject();
                        var row = sender as DataGridRow;
                        dataObject.SetData("Source", ((MyTask)row.DataContext).Id.ToString());
                        //var adLayer = AdornerLayer.GetAdornerLayer(item);
                        // myAdornment = new DraggableAdorner(item);
                        //adLayer.Add(myAdornment);
                        Debug.Print($"DragDrop.DoDragDrop");
                        DragDrop.DoDragDrop(row, dataObject, DragDropEffects.Move);
                        //adLayer.Remove(myAdornment);
                        e.Handled = true;
                    }
                }), null);
            }), CancellationToken.None);
        }

        private void Item_OnPreviewGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
        }

        private void Item_OnDrop(object sender, DragEventArgs e)
        {
            var source = e.Data.GetData("Source") as string;
            if (source != null)
            {
                var newIndex = view1.Items.IndexOf((sender as ListViewItem).Content);
                var list = view1.ItemsSource as ObservableCollection<string>;
                list.RemoveAt(list.IndexOf(source));
                list.Insert(newIndex, source);
            }
        }

        public static childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            // Search immediate children first (breadth-first)
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is childItem)
                    return (childItem)child;
                var childOfChild = FindVisualChild<childItem>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void DataGrid_OnDrop(object sender, DragEventArgs e)
        {
            _isDragging = false;
            var index = this.GetCurrentIndex(e.GetPosition);
            Debug.Print($"Drop. index: {index}");
            var source = int.Parse(e.Data.GetData("Source") as String);
            var items = (ObservableCollection<MyTask>)view1.ItemsSource;
            var old = items.Select((a, i)=> new {Index=i, Data=a}).FirstOrDefault(a=> a.Data.Id == source);
            if (old != null)
            {
                if (old.Index < index) index--;
                items.RemoveAt(old.Index);
                items.Insert(index, old.Data);
            }
        }

        delegate Point GetPositionDelegate(IInputElement element);
        int GetCurrentIndex(GetPositionDelegate getPosition)
        {
            bool isUp = false;
            for (var i = 0; i < view1.Items.Count; i++)
            {
                var row = (DataGridRow)view1.ItemContainerGenerator.ContainerFromIndex(i);
                var offset = MouseOverTargetOffset(row, getPosition, out isUp);
                if (offset.HasValue)
                    return i + offset.Value;
            }
            return isUp ? 0 : view1.Items.Count;
        }

        /*int GetCurrentIndex(GetPositionDelegate getPosition)
        {
            for (var i = 0; i < view1.Items.Count; i++)
            {
                var row = (DataGridRow)view1.ItemContainerGenerator.ContainerFromIndex(i);
                if (IsMouseOverTarget(row, getPosition))
                    return i;
            }
            return -1;
        }*/

        bool IsMouseOverTarget(Visual target, GetPositionDelegate getPosition)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            Point mousePos = getPosition((IInputElement)target);
            if (bounds.Contains(mousePos))
            {

            }
            return bounds.Contains(mousePos);
        }

        int? MouseOverTargetOffset(Visual target, GetPositionDelegate getPosition, out bool isUp)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            Point mousePos = getPosition((IInputElement)target);
            isUp = false;
            if (bounds.Contains(mousePos))
            {
                return mousePos.Y < bounds.Top + bounds.Height / 2 ? 0 : 1;
            }

            isUp = mousePos.Y < 0;
            return null;
        }

        //===============================
        //===============================
        //===============================
        private void View2_OnDrop(object sender, DragEventArgs e)
        {
            _isDragging = false;
            var index = this.GetCurrentIndex(e.GetPosition);
            Debug.Print($"Drop2. index: {index}");
            var source = int.Parse(e.Data.GetData("Source") as String);
            var items = (ObservableCollection<MyTask>)view1.ItemsSource;
            var old = items.Select((a, i) => new { Index = i, Data = a }).FirstOrDefault(a => a.Data.Id == source);
            if (old != null)
            {
                var items2 = (ObservableCollection<MyTask>)view2.ItemsSource;
                items2.Add(old.Data);
            }
        }
    }
}
