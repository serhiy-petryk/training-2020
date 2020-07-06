using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using MyWpfMwi.Common;

namespace MyWpfMwi.Mwi
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
        private void AnimateWindowState(WindowState previousWindowState)
        {
            Rect to;
            if (WindowState == WindowState.Normal)
                to = new Rect(_attachedPosition.X, _attachedPosition.Y, _lastNormalSize.Width, _lastNormalSize.Height);
            else if (WindowState == WindowState.Maximized)
                to = new Rect(0, 0, Container.ActualWidth, Container.InnerHeight);
            else
                to = new Rect(Position.X, 0, ActualWidth, MinHeight);

            var opacityValues = previousWindowState == WindowState.Minimized
                ? Tuple.Create(0.0, 1.0)
                : (WindowState == WindowState.Minimized ? Tuple.Create(1.0, 0.0) : null);

            SetRectangleWithAnimation(to, () =>
            {
                if (WindowState == WindowState.Minimized)
                {
                    Visibility = Visibility.Collapsed;
                    if (Container.ActiveMwiChild == this)
                    {
                        var newMwiChild = Container.InternalWindows.Any(w => w.WindowState != WindowState.Minimized)
                            ? Container.InternalWindows.Where(w => w.WindowState != WindowState.Minimized)
                                .Aggregate((w1, w2) => Panel.GetZIndex(w1) > Panel.GetZIndex(w2) ? w1 : w2)
                            : null;
                        if (newMwiChild != null)
                            Container.ActiveMwiChild = newMwiChild;
                    }
                }
            }, opacityValues);

            _sbWindowState.Begin();
        }

        // =============================
        private void CreateWindowStateStoryboard()
        {
            if (_sbWindowState == null)
            {
                _sbWindowState = new Storyboard();
                _sbWindowState.Children.Add(AnimationHelper.GetPositionAnimation(this, new Point(), new Point(), FillBehavior.Stop));
                _sbWindowState.Children.Add(AnimationHelper.GetWidthAnimation(this, 0, 0, FillBehavior.Stop));
                _sbWindowState.Children.Add(AnimationHelper.GetHeightAnimation(this, 0, 0, FillBehavior.Stop));
                _sbWindowState.Children.Add(AnimationHelper.GetOpacityAnimation(this, 0, 0, FillBehavior.Stop));
                _sbWindowState.Children[0].Completed += (o, args) =>
                {
                    _sbWindowStateCallback?.Invoke();
                    _sbWindowStateCallback = null;
                };
            }
        }
    }
}
