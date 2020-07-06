using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace IconViewer
{
    /// <summary>
    /// Interaction logic for ScrollCanvas.xaml
    /// </summary>
    public partial class ScrollCanvas
    {
        public ScrollCanvas()
        {
            InitializeComponent();
        }

        protected override Size MeasureOverride(System.Windows.Size constraint)
        {
            return new Size(400,400);
        }

        /*public static readonly DependencyProperty TopProperty = //DependencyProperty.RegisterAttached("Top", typeof(MwiContainer), typeof(ScrollCanvas));
        DependencyProperty.RegisterAttached("Top", typeof(double), typeof(Canvas), new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(Canvas.OnPositioningChanged)), new ValidateValueCallback(Shape.IsDoubleFiniteOrNaN));
        // public static void SetMwiContainer(DependencyObject element, MwiContainer value) => element?.SetValue(MwiContainerProperty, value); // NotNull propagation need to prevent VS designer error
        // public static MwiContainer GetMwiContainer(DependencyObject element) => element?.GetValue(MwiContainerProperty) as MwiContainer; // NotNull propagation need to prevent VS designer error

        public static readonly DependencyProperty BottomProperty;*/

        private static void OnPositioningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement uIElement = d as UIElement;
            if (uIElement != null)
            {
                Canvas canvas = VisualTreeHelper.GetParent(uIElement) as Canvas;
                if (canvas != null)
                {
                    canvas.InvalidateArrange();
                }
            }
        }
    }
}
