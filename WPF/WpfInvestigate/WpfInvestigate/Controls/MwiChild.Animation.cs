using System;
using System.Windows;
using System.Windows.Media.Animation;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    public partial class MwiChild
    {
        private Storyboard _sbWindowState;
        private Action _sbWindowStateCallback;

        public void SetRectangleWithAnimation(Rect to, Action callback = null, Tuple<double, double> opacity = null)
        {
            CreateWindowStateStoryboard();

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
            _sbWindowStateCallback = callback;

            _sbWindowState.Begin();
        }

        private void AnimateShow(Action callback = null) => SetRectangleWithAnimation(new Rect(Position.X, Position.Y, ActualWidth, ActualHeight), callback, new Tuple<double, double>(0, 1));
        private void AnimateHide(Action callback = null) => SetRectangleWithAnimation(new Rect(Position.X, Position.Y, ActualWidth, ActualHeight), callback, new Tuple<double, double>(1, 0));

        // =============================
        private void CreateWindowStateStoryboard()
        {
            if (_sbWindowState == null)
            {
                _sbWindowState = new Storyboard();
                _sbWindowState.Children.Add(AnimationHelper.CreateAnimation(this, PositionProperty));
                _sbWindowState.Children.Add(AnimationHelper.CreateAnimation(this, WidthProperty));
                _sbWindowState.Children.Add(AnimationHelper.CreateAnimation(this, HeightProperty));
                _sbWindowState.Children.Add(AnimationHelper.CreateAnimation(this, OpacityProperty));
                _sbWindowState.Children[0].Completed += (o, args) =>
                {
                    _sbWindowStateCallback?.Invoke();
                    _sbWindowStateCallback = null;
                };
            }
        }

    }
}
