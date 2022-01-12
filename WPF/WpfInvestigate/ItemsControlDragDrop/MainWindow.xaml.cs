using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ItemsControlDragDrop.Code;

namespace ItemsControlDragDrop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            this.view1.ItemsSource = MyTask.CreateTasks();
            this.view2.ItemsSource = new ObservableCollection<MyTask>();

        }

        private void View1_OnPreviewMouseMove(object sender, MouseEventArgs e) => DragDropHelper.DragSource_OnPreviewMouseMove(sender, e);
        private void View1_OnPreviewDrop(object sender, DragEventArgs e) => DragDropHelper.DropTarget_OnPreviewDrop(sender, e);

        // private void View1_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragDropHelper.DragSource_OnPreviewMouseLeftButtonDown(sender, e);

        private void View1_OnPreviewDragOver(object sender, DragEventArgs e) => DragDropHelper.DropTarget_OnPreviewDragOver(sender, e);

        private void View1_OnPreviewDragLeave(object sender, DragEventArgs e) => DragDropHelper.DropTarget_OnPreviewDragLeave(sender, e);

        private void View1_OnPreviewDragEnter(object sender, DragEventArgs e) => DragDropHelper.DropTarget_OnPreviewDragOver(sender, e);
    }
}
