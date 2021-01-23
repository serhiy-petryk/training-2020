using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    public partial class MwiChild
    {
        private Storyboard _sbWindowState;
        public Task SetRectangleWithAnimation(Rect to, Tuple<double, double> opacity = null)
        {
            if (_sbWindowState == null)
            {
                _sbWindowState = new Storyboard();
                foreach (var timeline in AnimationHelper.CreateAnimations(this, PositionProperty, WidthProperty, HeightProperty, OpacityProperty))
                    _sbWindowState.Children.Add(timeline);
            }

            var from = new Rect(Position.X, Position.Y, ActualWidth, ActualHeight);
            // To restore window rect after animation
            Position = new Point(to.Left, to.Top);
            Width = to.Width;
            Height = to.Height;

            ((PointAnimation)_sbWindowState.Children[0]).From = new Point(from.Left, from.Top);
            ((PointAnimation)_sbWindowState.Children[0]).To = new Point(to.Left, to.Top);
            ((DoubleAnimation)_sbWindowState.Children[1]).From = from.Width;
            ((DoubleAnimation)_sbWindowState.Children[1]).To = to.Width;
            ((DoubleAnimation)_sbWindowState.Children[2]).From = from.Height;
            ((DoubleAnimation)_sbWindowState.Children[2]).To = to.Height;
            ((DoubleAnimation)_sbWindowState.Children[3]).From = opacity?.Item1 ?? 1;
            ((DoubleAnimation)_sbWindowState.Children[3]).To = opacity?.Item2 ?? 1;

            return _sbWindowState.BeginAsync(this);
        }

        private Task AnimateShow() => SetRectangleWithAnimation(new Rect(Position.X, Position.Y, ActualWidth, ActualHeight), new Tuple<double, double>(0, 1));
        private Task AnimateHide() => SetRectangleWithAnimation(new Rect(Position.X, Position.Y, ActualWidth, ActualHeight), new Tuple<double, double>(1, 0));
    }
}
