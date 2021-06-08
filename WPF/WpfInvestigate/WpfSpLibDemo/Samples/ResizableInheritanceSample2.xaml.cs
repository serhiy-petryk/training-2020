using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using WpfSpLib.Common;

namespace WpfSpLibDemo.Samples
{
    /// <summary>
    /// Interaction logic for ResizableInheritanceSample2.xaml
    /// </summary>
    public partial class ResizableInheritanceSample2
    {
        static ResizableInheritanceSample2()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizableInheritanceSample2), new FrameworkPropertyMetadata(typeof(ResizableInheritanceSample2)));
        }

        public ResizableInheritanceSample2()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += ResizableInheritanceSample2_Loaded;
        }

        private void ResizableInheritanceSample2_Loaded(object sender, RoutedEventArgs e)
        {
            // Error for DLL, because line
            // 'DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizingControl), new FrameworkPropertyMetadata(typeof(ResizingControl)))'
            // in ResizingControl.cs not commented 
            /* Loaded -= ResizableInheritanceSample2_Loaded;
            //Dispatcher.Invoke(() =>
            //{
                var cp = Tips.GetVisualChildren(this).OfType<ContentPresenter>().FirstOrDefault();
                var a2 = cp.ContentTemplate.FindName("MovingThumb", cp) as Thumb;
                MovingThumb = a2;
            //}, DispatcherPriority.DataBind);*/
        }
    }
}
