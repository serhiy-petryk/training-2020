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
                {
                    timeline.FillBehavior = FillBehavior.Stop;
                    _sbWindowState.Children.Add(timeline);
                }
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

        private async void AnimateWindowState(WindowState previousWindowState)
        {
            Rect to;
            if (WindowState == WindowState.Normal)
                to = new Rect(_attachedPosition.X, _attachedPosition.Y, _lastNormalSize.Width, _lastNormalSize.Height);
            else if (WindowState == WindowState.Maximized)
            {
                /*if (IsDialog)
                {
                    var itemsPresenter = ((DialogItems)Parent).ItemsPresenter;
                    var container = itemsPresenter == null ? null : VisualTreeHelper.GetParent(itemsPresenter) as FrameworkElement;
                    to = new Rect(0, 0, container.ActualWidth, container.ActualHeight);
                }
                else*/
                    to = new Rect(0, 0, MwiContainer.ActualWidth, MwiContainer.InnerHeight);
            }
            else
                to = new Rect(Position.X, 0, ActualWidth, MinHeight);

            await SetRectangleWithAnimation(to, previousWindowState == WindowState.Minimized ? 0 : 1,
                WindowState == WindowState.Minimized ? 0 : 1);

            /*if (WindowState == WindowState.Minimized)
            {
                Visibility = Visibility.Collapsed;
                if (MwiContainer.ActiveMwiChild == this)
                {
                    var newMwiChild = MwiContainer.InternalWindows.Any(w => w.WindowState != WindowState.Minimized)
                        ? MwiContainer.InternalWindows.Where(w => w.WindowState != WindowState.Minimized)
                            .Aggregate((w1, w2) => Panel.GetZIndex(w1) > Panel.GetZIndex(w2) ? w1 : w2)
                        : null;
                    if (newMwiChild != null)
                        MwiContainer.ActiveMwiChild = newMwiChild;
                }
            }*/
        }


    }
}
