using System.Linq;
using System.Windows.Controls.Primitives;
using WpfInvestigate.Common;

namespace WpfInvestigate.Samples
{
    /// <summary>
    /// Interaction logic for ResizableInheritanceSample3.xaml
    /// </summary>
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

        // ==============================
        /*public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content),
            typeof(object), typeof(ResizableInheritanceSample3), new UIPropertyMetadata(null));
        public new object Content
        {
            get => (object)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }*/

        /*public static readonly DependencyProperty XXContentProperty = DependencyProperty.Register(nameof(XXContent),
            typeof(object), typeof(ResizableInheritanceSample3), new UIPropertyMetadata(null));
        public object XXContent
        {
            get => (object)GetValue(XXContentProperty);
            set => SetValue(XXContentProperty, value);
        }*/

    }
}
