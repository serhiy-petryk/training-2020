using System.Windows;
using WpfInvestigate.Controls;
using WpfInvestigate.Samples;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for ResizableControl.xaml
    /// </summary>
    public partial class ResizableControlTests : Window
    {
        public ResizableControlTests()
        {
            InitializeComponent();

            var resizableControl = new ResizableControl
                {Content = new ResizableContentTemplateSample(), Margin = new Thickness(200, 100, 0, 0)};
            GridPanel.Children.Add(resizableControl);
        }
    }
}
