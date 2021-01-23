using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    public partial class MwiChild
    {
        private Storyboard _sbWindowState;
        public Task SetRectangleWithAnimation(Rect to, double fromOpacity, double toOpacity)
        {
            if (_sbWindowState == null)
            {
                _sbWindowState = new Storyboard();
                foreach (var timeline in AnimationHelper.CreateAnimations(this, PositionProperty, WidthProperty, HeightProperty, OpacityProperty))
                    _sbWindowState.Children.Add(timeline);
            }

            var from = new Rect(Position.X, Position.Y, ActualWidth, ActualHeight);

            _sbWindowState.Children[0].SetFromToValues(new Point(from.Left, from.Top), new Point(to.Left, to.Top));
            _sbWindowState.Children[1].SetFromToValues(from.Width, to.Width);
            _sbWindowState.Children[2].SetFromToValues(from.Height, to.Height);
            _sbWindowState.Children[3].SetFromToValues(fromOpacity, toOpacity);

            // To restore window rect after animation
            Position = new Point(to.Left, to.Top);
            Width = to.Width;
            Height = to.Height;
            Opacity = toOpacity;

            return _sbWindowState.BeginAsync(this);
        }

        private Task AnimateShow() => SetRectangleWithAnimation(new Rect(Position.X, Position.Y, ActualWidth, ActualHeight), 0, 1);
        private Task AnimateHide() => SetRectangleWithAnimation(new Rect(Position.X, Position.Y, ActualWidth, ActualHeight), 1, 0);
    }
}
