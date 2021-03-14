using System.Windows;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for MwiTemplate.xaml
    /// </summary>
    public partial class MwiTemplate : Window
    {
        public MwiTemplate()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
