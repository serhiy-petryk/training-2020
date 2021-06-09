using System.Windows;
using WpfSpLib.Controls;

namespace WpfSpLibDemo.Samples
{
    /// <summary>
    /// Interaction logic for ResizableInheritanceSample3.xaml
    /// </summary>
    public partial class ResizableInheritanceSample3: ResizingControl
    {
        static ResizableInheritanceSample3()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizableInheritanceSample3), new FrameworkPropertyMetadata(typeof(ResizableInheritanceSample3)));
            FocusableProperty.OverrideMetadata(typeof(ResizableInheritanceSample3), new FrameworkPropertyMetadata(true));
        }

        public ResizableInheritanceSample3()
        {
            InitializeComponent();
            DataContext = this;
        }

        // ==============================
        public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content),
            typeof(object), typeof(ResizableInheritanceSample3), new FrameworkPropertyMetadata(null));
        public new object Content
        {
            get => (object)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        //=============================
        public static readonly DependencyProperty BaseContentProperty = DependencyProperty.Register(nameof(BaseContent),
            typeof(object), typeof(ResizableInheritanceSample3),
            new FrameworkPropertyMetadata(null, (o, args) => ((ResizableInheritanceSample3) o).SetBaseContent(args.NewValue)));
        public object BaseContent
        {
            get => GetValue(BaseContentProperty);
            set => SetValue(BaseContentProperty, value);
        }
        private void SetBaseContent(object content) => base.Content = content;

    }
}
