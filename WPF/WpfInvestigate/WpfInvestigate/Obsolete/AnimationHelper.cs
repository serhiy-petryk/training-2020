using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace WpfInvestigate.Obsolete
{
    class AnimationHelper
    {
        public static TimelineCollection[] GetPathAnimations(Path element, Geometry from, Geometry to)
        {
            var result = new[] { new TimelineCollection(), new TimelineCollection() };
            result[0].Add(Common.AnimationHelper.GetFrameAnimation(element, Path.DataProperty, to));
            result[1].Add(Common.AnimationHelper.GetFrameAnimation(element, Path.DataProperty, from));

            var widthPart1Animation = Common.AnimationHelper.GetWidthAnimation(element, element.Width, 0);
            var widthPart2Animation = Common.AnimationHelper.GetWidthAnimation(element, 0, element.Width);
            widthPart2Animation.BeginTime = Common.AnimationHelper.AnimationDuration.TimeSpan;
            widthPart2Animation.Duration = Common.AnimationHelper.AnimationDuration;
            result[0].Add(widthPart1Animation);
            result[0].Add(widthPart2Animation);
            result[1].Add(widthPart1Animation);
            result[1].Add(widthPart2Animation);

            return result;
        }

    }
}
