using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using WpfInvestigate.Common;

namespace WpfInvestigate.Samples
{
    /// <summary>
    /// Interaction logic for ResizableInheritanceSample.xaml
    /// </summary>
    public partial class ResizableInheritanceSample
    {
        public ResizableInheritanceSample()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += ResizableInheritanceSample_Loaded;
        }

        private void ResizableInheritanceSample_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ResizableInheritanceSample_Loaded;
            //Dispatcher.Invoke(() =>
            //{
                var a1 = Template.FindName("MovingThumb", this);
                var cp = Tips.GetVisualChildren(this).OfType<ContentPresenter>().FirstOrDefault();
                var a2 = cp.ContentTemplate.FindName("MovingThumb", cp);
                MovingThumb = a2 as Thumb;
            //}, DispatcherPriority.DataBind);
        }

        // ==============================
        /*public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content),
            typeof(object), typeof(ResizableInheritanceSample), new UIPropertyMetadata(null));
        public new object Content
        {
            get => (object)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }*/

        /*public static readonly DependencyProperty XXContentProperty = DependencyProperty.Register(nameof(XXContent),
            typeof(object), typeof(ResizableInheritanceSample), new UIPropertyMetadata(null));
        public object XXContent
        {
            get => (object)GetValue(XXContentProperty);
            set => SetValue(XXContentProperty, value);
        }*/

    }
}
