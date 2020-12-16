using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using WpfInvestigate.Helpers;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for ShadowEffectTests.xaml
    /// </summary>
    public partial class ShadowEffectTests : Window
    {
        public ShadowEffectTests()
        {
            InitializeComponent();
        }

        private void CreateClip_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var rect = new Rect{Width = button.Width, Height = button.Height};
            var a2 = ControlHelper.GetRoundRectangle(rect, new Thickness(), ControlHelper.GetCornerRadius(button).Value);
            var a3 = ControlHelper.GetRoundRectangle(rect, new Thickness(15), ControlHelper.GetCornerRadius(button).Value);
            if (button.Effect is DropShadowEffect)
            {
                var shadow = (DropShadowEffect) button.Effect;
                var mi = shadow.GetType().GetMethod("GetRenderBounds", BindingFlags.Instance | BindingFlags.NonPublic);
                var shadowRect = (Rect)mi.Invoke(shadow, new object[] {new Rect {Width = button.ActualWidth, Height = button.ActualHeight}});
                var rectGeometry = new RectangleGeometry(shadowRect);

                var combined = new CombinedGeometry(GeometryCombineMode.Exclude, rectGeometry, a2);
                button.Clip = combined;
            }
            // button.Clip = a2;
        }
    }
}
