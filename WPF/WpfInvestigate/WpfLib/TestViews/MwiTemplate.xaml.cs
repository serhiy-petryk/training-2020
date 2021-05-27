using System.Windows;

namespace WpfLib.TestViews
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
