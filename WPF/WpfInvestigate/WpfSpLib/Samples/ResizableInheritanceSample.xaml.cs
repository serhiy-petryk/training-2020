using System.Linq;
using System.Windows.Controls.Primitives;
using WpfSpLib.Common;

namespace WpfSpLib.Samples
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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            MovingThumb = Tips.GetVisualChildren(this).OfType<Thumb>().FirstOrDefault(t => t.Name == MovingThumbName);
        }

        // ==============================
        /*public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content),
            typeof(object), typeof(ResizableInheritanceSample), new FrameworkPropertyMetadata(null));
        public new object Content
        {
            get => (object)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }*/

        /*public static readonly DependencyProperty XXContentProperty = DependencyProperty.Register(nameof(XXContent),
            typeof(object), typeof(ResizableInheritanceSample), new FrameworkPropertyMetadata(null));
        public object XXContent
        {
            get => (object)GetValue(XXContentProperty);
            set => SetValue(XXContentProperty, value);
        }*/

    }
}
