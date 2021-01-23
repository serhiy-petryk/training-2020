using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using WpfInvestigate.Common;

namespace WpfInvestigate.Obsolete
{
    class AnimationHelper
    {
        public static TimelineCollection[] GetPathAnimations(Path element, Geometry from, Geometry to)
        {
            var result = new[] { new TimelineCollection(), new TimelineCollection() };
            result[0].Add(Common.AnimationHelper.CreateFrameAnimation(element, Path.DataProperty, to));
            result[1].Add(Common.AnimationHelper.CreateFrameAnimation(element, Path.DataProperty, from));

            var widthPartAnimations = Common.AnimationHelper.CreateAnimations(element, FrameworkElement.WidthProperty, FrameworkElement.WidthProperty);
            widthPartAnimations[0].SetFromToValues(element.Width, 0.0);
            widthPartAnimations[1].SetFromToValues(0.0, element.Width);
            widthPartAnimations[1].BeginTime = Common.AnimationHelper.AnimationDuration.TimeSpan;

            result[0].Add(widthPartAnimations[0]);
            result[0].Add(widthPartAnimations[1]);
            result[1].Add(widthPartAnimations[0]);
            result[1].Add(widthPartAnimations[1]);

            return result;
        }
    }
}
