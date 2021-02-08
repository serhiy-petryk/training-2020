using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    public partial class MwiChild
    {
        private Task SetRectWithAnimation(Rect to, double fromOpacity, double toOpacity)
        {
            var tasks = new List<Task>
            {
                this.BeginAnimationAsync(PositionProperty, ActualPosition, new Point(to.X, to.Y)),
                this.BeginAnimationAsync(WidthProperty, ActualWidth, to.Width),
                this.BeginAnimationAsync(HeightProperty, ActualHeight, to.Height),
                this.BeginAnimationAsync(OpacityProperty, fromOpacity, toOpacity)
            };

            return Task.WhenAll(tasks.ToArray());
        }

        private Task AnimateShow()
        {
            if (MwiContainer != null && (Position.X < 0 || Position.Y < 0))
                Position = MwiContainer.GetStartPositionForMwiChild(this);

            return SetRectWithAnimation(new Rect(ActualPosition.X, ActualPosition.Y, ActualWidth, ActualHeight), 0, 1);
        }

        private Task AnimateHide() => SetRectWithAnimation(new Rect(ActualPosition.X, ActualPosition.Y, ActualWidth, ActualHeight), 1, 0);

        private async Task AnimateWindowState(WindowState previousWindowState)
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
                    to = new Rect(0, 0, HostPanel.ActualWidth, HostPanel.ActualHeight);
            }
            else
                to = new Rect(ActualPosition.X, 0, ActualWidth, MinHeight);

            await SetRectWithAnimation(to, previousWindowState == WindowState.Minimized ? 0 : 1, WindowState == WindowState.Minimized ? 0 : 1);

            if (WindowState == WindowState.Minimized)
            {
                Visibility = Visibility.Collapsed;
                if (MwiContainer.ActiveMwiChild == this)
                    MwiContainer.GetTopChild(MwiContainer.InternalWindows.Where(w => w.WindowState != WindowState.Minimized))?.Activate();
            }
        }


    }
}
