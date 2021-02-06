using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Interaction logic for MwiItems.xaml
    /// </summary>
    public class MwiItems: ItemsControl
    {
        public MwiItems()
        {
            //ClearValue(ItemsControl.ItemsSourceProperty);
            //ItemsSource = null;
            //ItemsSource = new ObservableCollection<MwiChild>();
            // InitializeComponent();
            DataContext = this;
        }
    }
}
