using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Interaction logic for MwiItems.xaml
    /// </summary>
    public class MwiItems: ItemsControl, INotifyPropertyChanged
    {
        static MwiItems()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MwiItems), new FrameworkPropertyMetadata(typeof(MwiItems)));
        }

        public MwiItems()
        {
            DataContext = this;
            ((INotifyCollectionChanged)Items).CollectionChanged += OnItemsChanged; 
        }

        private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Debug.Print($"OnItemsChanged. {e.Action}");
        }

        private MwiChild _activeMwiChild;
        public MwiChild ActiveMwiChild
        {
            get => _activeMwiChild;
            set
            {
                if (!Equals(_activeMwiChild, value)) _activeMwiChild = value;
                OnPropertiesChanged(nameof(ActiveMwiChild), nameof(ScrollBarKind));
            }
        }

        public ScrollBarVisibility ScrollBarKind =>
            ActiveMwiChild != null && ActiveMwiChild.WindowState == WindowState.Maximized
                ? ScrollBarVisibility.Disabled
                : ScrollBarVisibility.Auto;

        #region =================  INotifyPropertyChanged  ==================
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}
