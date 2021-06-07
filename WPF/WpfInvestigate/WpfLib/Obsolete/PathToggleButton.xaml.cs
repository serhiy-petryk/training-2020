using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using WpfLib.Common;

namespace WpfLib.Obsolete
{
    /// <summary>
    /// Interaction logic for PathToggleButton.xaml
    /// </summary>
    public partial class PathToggleButton
    {
        static PathToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PathToggleButton), new FrameworkPropertyMetadata(typeof(PathToggleButton)));
        }

        public PathToggleButton()
        {
            InitializeComponent();
        }

        public double ViewBoxWidth => Width - (Padding.Left + Padding.Right) - 2;
        public double ViewBoxHeight => Height - (Padding.Top + Padding.Bottom) - 2;

        public static readonly DependencyProperty PathWidthProperty = DependencyProperty.Register("PathWidth", typeof(double), typeof(PathToggleButton), new FrameworkPropertyMetadata(24.0));
        public double PathWidth
        {
            get => (double)GetValue(PathWidthProperty);
            set => SetValue(PathWidthProperty, value);
        }
        public static readonly DependencyProperty PathHeightProperty = DependencyProperty.Register("PathHeight", typeof(double), typeof(PathToggleButton), new FrameworkPropertyMetadata(24.0));
        public double PathHeight
        {
            get => (double)GetValue(PathHeightProperty);
            set => SetValue(PathHeightProperty, value);
        }
        public static readonly DependencyProperty GeometryOnProperty = DependencyProperty.Register("GeometryOn", typeof(Geometry), typeof(PathToggleButton));
        public Geometry GeometryOn
        {
            get => (Geometry)GetValue(GeometryOnProperty);
            set => SetValue(GeometryOnProperty, value);
        }
        public static readonly DependencyProperty GeometryOffProperty = DependencyProperty.Register("GeometryOff", typeof(Geometry), typeof(PathToggleButton));
        public Geometry GeometryOff
        {
            get => (Geometry)GetValue(GeometryOffProperty);
            set => SetValue(GeometryOffProperty, value);
        }

        public static readonly RoutedEvent BeforeAnimateEvent = EventManager.RegisterRoutedEvent("BeforeAnimate", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PathToggleButton));
        public event RoutedEventHandler BeforeAnimate
        {
            add => AddHandler(BeforeAnimateEvent, value);
            remove => RemoveHandler(BeforeAnimateEvent, value);
        }

        //======================================
        private void PathToggleButton_OnLoaded(object sender, RoutedEventArgs e) => path.Data = IsChecked == true ? GeometryOn : GeometryOff;

        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);
            Checked_OnChanged();
        }
        protected override void OnUnchecked(RoutedEventArgs e)
        {
            base.OnUnchecked(e);
            Checked_OnChanged();
        }

        private void Checked_OnChanged()
        {
            if (!IsLoaded)
                return;

            var sbs = Resources["Animation"] as Storyboard[];
            var isFirstRun = (sbs == null);
            if (isFirstRun)
            {
                PrepareToggleButtonAnimation();
                sbs = (Storyboard[])Resources["Animation"];
            }

            RaiseEvent(new BeforeAnimateEventArgs(BeforeAnimateEvent, sbs, isFirstRun));

            sbs[IsChecked == true ? 0 : 1].Begin();
        }

        private void PrepareToggleButtonAnimation()
        {
            var sbFirst = new Storyboard();
            var sbSecond = new Storyboard();

            var buttonPath = Tips.GetVisualChildren(this).First(c=> c is Path) as Path;
            buttonPath.Width = PathWidth;
            buttonPath.Height = PathHeight;
            var aa = Obsolete.AnimationHelper.GetPathAnimations(buttonPath, GeometryOff, GeometryOn);
            foreach (var a in aa[0])
                sbFirst.Children.Add(a);
            foreach (var a in aa[1])
                sbSecond.Children.Add(a);
            Resources.Add("Animation", new[] { sbFirst, sbSecond });
        }
    }
}
