using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WpfInvestigate.Common;
using WpfInvestigate.Helpers;
using WpfInvestigate.Samples;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for DragDropTests.xaml
    /// </summary>
    public partial class DragDropTests : Window
    {
        public DragDropTests()
        {
            InitializeComponent();
            DataContext = this;

            view1.ItemsSource = MyTask.CreateTasks();
            view2.ItemsSource = new ObservableCollection<MyTask>();
        }
        private void View1_OnPreviewMouseMove(object sender, MouseEventArgs e) => DragDropHelper.DragSource_OnPreviewMouseMove(sender, e);
        private void View1_OnPreviewDrop(object sender, DragEventArgs e) => DragDropHelper.DropTarget_OnPreviewDrop(sender, e);

        // private void View1_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragDropHelper.DragSource_OnPreviewMouseLeftButtonDown(sender, e);

        private void View1_OnPreviewDragOver(object sender, DragEventArgs e)
        {
            Debug.Print($"DragOver: {((FrameworkElement)sender).Name}");
            DragDropHelper.DropTarget_OnPreviewDragOver(sender, e);
        }

        private void View1_OnPreviewDragLeave(object sender, DragEventArgs e)
        {
            Debug.Print($"DragLeave: {((FrameworkElement)sender).Name}");
            DragDropHelper.DropTarget_OnPreviewDragLeave(sender, e);
        }

        private void View1_OnPreviewDragEnter(object sender, DragEventArgs e)
        {
            Debug.Print($"DragEnter: {((FrameworkElement)sender).Name}");
            DragDropHelper.DropTarget_OnPreviewDragOver(sender, e);
        }

        private void View2_OnPreviewDrop(object sender, DragEventArgs e) => DragDropHelper.DropTarget_OnPreviewDrop(sender, e);

        private void View1_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            return;
            var aa1 = GetElementsUnderMouseClick((UIElement)sender, e);
            var aa2 = aa1[0].GetVisualParents().OfType<FrameworkElement>();
            var aa3 = aa2.Select(a => new Point(a.ActualWidth, a.ActualHeight));
        }

        public static List<DependencyObject> GetElementsUnderMouseClick(UIElement sender, MouseButtonEventArgs e)
        {
            var hitTestResults = new List<DependencyObject>();
            VisualTreeHelper.HitTest(sender, null, result => GetHitTestResult(result, hitTestResults), new PointHitTestParameters(e.GetPosition(sender)));
            return hitTestResults;
        }
        private static HitTestResultBehavior GetHitTestResult(HitTestResult result, List<DependencyObject> hitTestResults)
        {
            // Add the hit test result to the list that will be processed after the enumeration.
            hitTestResults.Add(result.VisualHit);
            // Set the behavior to return visuals at all z-order levels.
            return HitTestResultBehavior.Continue;
        }

        private Cursor customCursor = null;
        private void View1_OnPreviewGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            Debug.Print($"GiveFeedback: {((FrameworkElement)sender).Name}, {e.Effects}");
            if (e.Effects == DragDropEffects.Copy)
            {
                //if (customCursor == null)
                //      customCursor = new Cursor(new FileStream("Earth.cur", FileMode.Open));

                e.UseDefaultCursors = false;
                Mouse.SetCursor(Cursors.Arrow);
                // Mouse.SetCursor(customCursor);
            }
            else
                e.UseDefaultCursors = true;

            e.Handled = true;
        }
    }
}
