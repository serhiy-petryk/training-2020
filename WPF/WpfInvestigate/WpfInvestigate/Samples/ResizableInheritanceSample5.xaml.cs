using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WpfInvestigate.Controls;

namespace WpfInvestigate.Samples
{
    /// <summary>
    /// Interaction logic for ResizableInheritanceSample5.xaml
    /// </summary>
    public partial class ResizableInheritanceSample5
    {
        public ResizableInheritanceSample5()
        {
            InitializeComponent();
            DataContext = this;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var rControl = Template.FindName("ResizingControl", this) as FrameworkElement;
            if (rControl != null)
                SetBinding(Panel.ZIndexProperty, new Binding("(Panel.ZIndex)") { Source = rControl });
        }
    }
}
