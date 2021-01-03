using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Movable.Controls;

namespace Movable
{
    /// <summary>
    /// Interaction logic for MainWindowItemsControl.xaml
    /// </summary>
    public partial class MainWindowItemsControl : Window, INotifyPropertyChanged
    {
        public ObservableCollection<SampleDialogMovable> Data { get; set; } = new ObservableCollection<SampleDialogMovable>();
        public ObservableCollection<SampleDialogMovable> CanvasData { get; set; } = new ObservableCollection<SampleDialogMovable>();

        public MainWindowItemsControl()
        {
            InitializeComponent();
            Data.Add(new SampleDialogMovable());
            // ItemsControlCanvas.ItemsSource = Data;
            DataContext = this;

            var a1 = Canvas.Children;
        }

        private void AddChildToItemsControl_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = new SampleDialogMovable();
            // ItemsControlCanvas.Items.Add(a1);
            Data.Add(a1);
            OnPropertiesChanged(nameof(Data));
        }


        #region ===========  INotifyPropertyChanged  ===============
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}
