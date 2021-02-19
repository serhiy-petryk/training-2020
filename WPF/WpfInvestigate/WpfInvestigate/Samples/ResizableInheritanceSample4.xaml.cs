using System.Windows;

namespace WpfInvestigate.Samples
{
    /// <summary>
    /// Interaction logic for ResizableInheritanceSample4.xaml
    /// </summary>
    public partial class ResizableInheritanceSample4
    {
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
        /*
        //=============================
        public static readonly DependencyProperty BaseContentProperty = DependencyProperty.Register(nameof(BaseContent),
            typeof(object), typeof(ResizableInheritanceSample4),
            new FrameworkPropertyMetadata(null, (o, args) => ((ResizableInheritanceSample4) o).SetBaseContent(args.NewValue)));
        public object BaseContent
        {
            get => GetValue(BaseContentProperty);
            set => SetValue(BaseContentProperty, value);
        }
        private void SetBaseContent(object content) => base.Content = content;*/
    }
}
