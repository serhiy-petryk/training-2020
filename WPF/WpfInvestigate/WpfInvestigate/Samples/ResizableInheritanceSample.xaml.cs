using System.Windows;

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
        }

        // ==============================
        public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content),
            typeof(object), typeof(ResizableInheritanceSample), new UIPropertyMetadata(null));
        public new object Content
        {
            get => (object)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

    }
}
