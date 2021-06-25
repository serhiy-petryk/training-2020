using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WpfSpLibDemo.Samples
{
    /// <summary>
    /// Interaction logic for ResizableInheritanceSample5.xaml
    /// </summary>
    public partial class ResizableInheritanceSample5 : ContentControl
    {
        static ResizableInheritanceSample5()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizableInheritanceSample5), new FrameworkPropertyMetadata(typeof(ResizableInheritanceSample5)));
            FocusableProperty.OverrideMetadata(typeof(ResizableInheritanceSample5), new FrameworkPropertyMetadata(true));
        }

        public ResizableInheritanceSample5()
        {
            InitializeComponent();
            DataContext = this;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("ResizingControl") is FrameworkElement rControl)
                SetBinding(Panel.ZIndexProperty, new Binding("(Panel.ZIndex)") { Source = rControl });
        }
    }
}
