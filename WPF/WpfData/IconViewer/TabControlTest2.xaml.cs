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
    /// Interaction logic for TabControlTest2.xaml
    /// </summary>
    public partial class TabControlTest2 : Window
    {
        public TabControlTest2()
        {
            InitializeComponent();
        }

        bool isDragStarted = false;
        void tabcontrol_Drop(object sender, DragEventArgs e)
        {
            TabControl tabcontrol = sender as TabControl;
            TabItem draggeditem = e.Data.GetData(typeof(TabItem)) as TabItem;
            Point droppedPoint = e.GetPosition(tabcontrol);
            GeneralTransform transform;
            int index = -1;
            for (int i = 0; i < tabcontrol.Items.Count; i++)
            {
                TabItem item = tabcontrol.Items[i] as TabItem;
                transform = item.TransformToVisual(tabcontrol);
                Rect rect = transform.TransformBounds(new Rect() { X = 0, Y = 0, Width = item.ActualWidth, Height = item.ActualHeight });
                if (rect.Contains(droppedPoint))
                {
                    if (!item.Equals(draggeditem))
                    {
                        index = i;
                    }
                    break;
                }
            }
            if (index != -1)
            {
                tabcontrol.Items.Remove(draggeditem);
                tabcontrol.Items.Insert(index, draggeditem);
            }

            isDragStarted = false;
        }

        void tabcontrol_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TabItem)))
            {
                e.Effects = DragDropEffects.Move;
            }
        }

        void tabitem_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragStarted)
                return;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isDragStarted = true;
                TabItem item = sender as TabItem;
                TabControl tabcontrol = item.Parent as TabControl;
                tabcontrol.CaptureMouse();
                DragDrop.DoDragDrop(item, item, DragDropEffects.Copy);
            }
        }

        void tabitem_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                e.Action = DragAction.Cancel;
                isDragStarted = false;
            }
        }
    }
}
