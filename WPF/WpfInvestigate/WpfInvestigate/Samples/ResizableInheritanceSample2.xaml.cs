using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using WpfInvestigate.Common;

namespace WpfInvestigate.Samples
{
    /// <summary>
    /// Interaction logic for ResizableInheritanceSample2.xaml
    /// </summary>
    public partial class ResizableInheritanceSample2
    {
        public ResizableInheritanceSample2()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += ResizableInheritanceSample2_Loaded;
        }

        private void ResizableInheritanceSample2_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ResizableInheritanceSample2_Loaded;
            //Dispatcher.Invoke(() =>
            //{
                var cp = Tips.GetVisualChildren(this).OfType<ContentPresenter>().FirstOrDefault();
                var a2 = cp.ContentTemplate.FindName("MovingThumb", cp) as Thumb;
                MovingThumb = a2;
            //}, DispatcherPriority.DataBind);
        }

        // ==============================
        /*public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content),
            typeof(object), typeof(ResizableInheritanceSample2), new FrameworkPropertyMetadata(null));
        public new object Content
        {
            get => (object)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }*/

        /*public static readonly DependencyProperty XXContentProperty = DependencyProperty.Register(nameof(XXContent),
            typeof(object), typeof(ResizableInheritanceSample2), new FrameworkPropertyMetadata(null));
        public object XXContent
        {
            get => (object)GetValue(XXContentProperty);
            set => SetValue(XXContentProperty, value);
        }*/

    }
}
