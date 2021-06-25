using System.Windows;
using WpfSpLib.Controls;

namespace WpfSpLibDemo.Samples
{
    /// <summary>
    /// Interaction logic for ResizableInheritanceSample4.xaml
    /// </summary>
    public partial class ResizableInheritanceSample4 : ResizingControl
    {
        static ResizableInheritanceSample4()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizableInheritanceSample4), new FrameworkPropertyMetadata(typeof(ResizableInheritanceSample4)));
            FocusableProperty.OverrideMetadata(typeof(ResizableInheritanceSample4), new FrameworkPropertyMetadata(true));
        }

        public ResizableInheritanceSample4()
        {
            InitializeComponent();
            DataContext = this;
        }

        // ==============================
        public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content),
            typeof(object), typeof(ResizableInheritanceSample4), new FrameworkPropertyMetadata(null));
        public new object Content
        {
            get => (object)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }
    }
}
