using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Movable.Controls;
using Movable.Effects;

namespace Movable
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddChildToCanvas_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = new SampleDialogMovable();
            Canvas.Children.Add(a1);
        }

        private void AddChildToGrid_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = new SampleDialogMovable();
            Grid.Children.Add(a1);
        }

        private void AddChildToItemsControl_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = new SampleDialogMovable();
            ItemsControl.Items.Add(a1);
        }

        private void AddChildToItemsControlGrid_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = new SampleDialogMovable();
            ItemsControlGrid.Items.Add(a1);
        }

        private void AddChildToItemsControlCanvas_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = new SampleDialogMovable();
            ItemsControlCanvas.Items.Add(a1);
        }

        private void AddChildToScrollViewerGrid_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = new SampleDialogMovable();
            ScrollViewerGrid.Children.Add(a1);
        }

        private void AddChildToScrollViewerCanvas_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = new SampleDialogMovable();
            ScrollViewerCanvas.Children.Add(a1);
        }

        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var a1 = sender as FrameworkElement;
            var layer = AdornerLayer.GetAdornerLayer(a1);
        }

        private void AddChildToScrollGridAdorner_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = new SampleResizingContent();
            ScrollGridAdornerPanel.Children.Add(a1);
            ResizeEffect.SetEdgeThickness(a1, new Thickness(6));
        }
    }
}
