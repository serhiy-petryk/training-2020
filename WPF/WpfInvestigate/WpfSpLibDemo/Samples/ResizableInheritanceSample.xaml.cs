using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using WpfSpLib.Common;

namespace WpfSpLibDemo.Samples
{
    /// <summary>
    /// Interaction logic for ResizableInheritanceSample.xaml
    /// </summary>
    public partial class ResizableInheritanceSample
    {
        static ResizableInheritanceSample()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizableInheritanceSample), new FrameworkPropertyMetadata(typeof(ResizableInheritanceSample)));
            FocusableProperty.OverrideMetadata(typeof(ResizableInheritanceSample), new FrameworkPropertyMetadata(true));
        }

        public ResizableInheritanceSample()
        {
            InitializeComponent();
            DataContext = this;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            MovingThumb = Tips.GetVisualChildren(this).OfType<Thumb>().FirstOrDefault(t => t.Name == MovingThumbName);
        }
    }
}
