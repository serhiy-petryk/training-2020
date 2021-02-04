using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using WpfInvestigate.Common;

namespace WpfInvestigate.Samples
{
    /// <summary>
    /// Interaction logic for ResizableInheritanceSample3.xaml
    /// </summary>
    // [ContentProperty("XXContent")]
    public partial class ResizableInheritanceSample3
    {
        public ResizableInheritanceSample3()
        {
            InitializeComponent();
            DataContext = this;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            MovingThumb = Tips.GetVisualChildren(this).OfType<Thumb>().FirstOrDefault(t => t.Name == MovingThumbName);
        }

        // ==============  Not need  ================
        public new static readonly DependencyProperty XXContentProperty = DependencyProperty.Register(nameof(XXContent),
            typeof(object), typeof(ResizableInheritanceSample3), new UIPropertyMetadata(null));
        public new object XXContent
        {
            get => (object)GetValue(XXContentProperty);
            set => SetValue(XXContentProperty, value);
        }

        // ==============================
        public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content),
            typeof(object), typeof(ResizableInheritanceSample3), new UIPropertyMetadata(null));
        public new object Content
        {
            get => (object)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        //=============================
        public static readonly DependencyProperty BaseContentProperty = DependencyProperty.Register(nameof(BaseContent),
            typeof(FrameworkElement), typeof(ResizableInheritanceSample3),
            new UIPropertyMetadata(null, (o, args) => ((ResizableInheritanceSample3) o).SetBaseContent(args.NewValue)));
        public FrameworkElement BaseContent
        {
            get => (FrameworkElement)GetValue(BaseContentProperty);
            set => SetValue(BaseContentProperty, value);
        }
        private void SetBaseContent(object content) => base.Content = content;

    }
}
