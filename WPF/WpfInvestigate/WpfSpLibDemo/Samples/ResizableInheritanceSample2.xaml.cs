using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using WpfSpLib.Common;
using WpfSpLib.Controls;

namespace WpfSpLibDemo.Samples
{
    /// <summary>
    /// Interaction logic for ResizableInheritanceSample2.xaml
    /// </summary>
    public partial class ResizableInheritanceSample2: ResizingControl
    {
        static ResizableInheritanceSample2()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizableInheritanceSample2), new FrameworkPropertyMetadata(typeof(ResizableInheritanceSample2)));
            FocusableProperty.OverrideMetadata(typeof(ResizableInheritanceSample2), new FrameworkPropertyMetadata(true));
        }

        public ResizableInheritanceSample2()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += ResizableInheritanceSample2_Loaded;
        }

        private void ResizableInheritanceSample2_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ResizableInheritanceSample2_Loaded;
            var cp = this.GetVisualChildren().OfType<ContentPresenter>().FirstOrDefault();
            if (cp != null) // to prevent VS designer error
            {
                var a2 = cp.ContentTemplate.FindName("MovingThumb", cp) as Thumb;
                MovingThumb = a2;
            }
        }

    }
}
