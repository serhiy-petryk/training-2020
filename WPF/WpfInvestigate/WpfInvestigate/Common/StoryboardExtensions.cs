using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace WpfInvestigate.Common
{
    public static class StoryboardExtensions
    {
        public static Task<bool> BeginAsync(this Storyboard storyboard, FrameworkElement target)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (storyboard != null && target != null)
            {
                var temp = target.CacheMode;
                target.CacheMode = new System.Windows.Media.BitmapCache();
                var animation = storyboard.Clone();
                animation.Completed += (s, e) =>
                {
                    target.CacheMode = temp;
                    tcs.SetResult(true);
                };
                animation.Freeze();
                animation.Begin(target);
            }
            else
                tcs.SetResult(false);

            return tcs.Task;
        }

        public static Task BeginAnimationAsync(this IAnimatable animatable, DependencyProperty animationProperty, AnimationTimeline animation)
        {
            // state in constructor == animation cancel action
            var tcs = new TaskCompletionSource<bool>(new Action(() => animatable.BeginAnimation(animationProperty, null)));
            animation.Completed += (s, e) => tcs.SetResult(true);
            animation.Freeze();
            animatable.BeginAnimation(animationProperty, animation);
            return tcs.Task;
        }
    }
}
