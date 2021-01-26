using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    public partial class MwiChild
    {
        public Task SetRectangleWithAnimation(Rect to, double fromOpacity, double toOpacity)
        {
            var tasks = new List<Task>();
            if (!Tips.AreEqual(ActualPosition.X, to.X) || !Tips.AreEqual(ActualPosition.Y, to.Y))
                tasks.Add(this.BeginAnimationAsync(PositionProperty, ActualPosition, new Point(to.X, to.Y)));
            if (!Tips.AreEqual(ActualWidth, to.Width))
                tasks.Add(this.BeginAnimationAsync(WidthProperty, ActualWidth, to.Width));
            if (!Tips.AreEqual(ActualHeight, to.Height))
                tasks.Add(this.BeginAnimationAsync(HeightProperty, ActualHeight, to.Height));
            if (!Tips.AreEqual(fromOpacity, toOpacity))
                tasks.Add(this.BeginAnimationAsync(OpacityProperty, fromOpacity, toOpacity));

            return Task.WhenAll(tasks.ToArray());
        }

        private Task AnimateShow() => SetRectangleWithAnimation(new Rect(ActualPosition.X, ActualPosition.Y, ActualWidth, ActualHeight), 0, 1);
        private Task AnimateHide() => SetRectangleWithAnimation(new Rect(ActualPosition.X, ActualPosition.Y, ActualWidth, ActualHeight), 1, 0);

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
                    to = new Rect(0, 0, MwiContainer.ActualWidth, MwiContainer.InnerHeight);
            }
            else
                to = new Rect(ActualPosition.X, 0, ActualWidth, MinHeight);

            await SetRectangleWithAnimation(to, previousWindowState == WindowState.Minimized ? 0 : 1, WindowState == WindowState.Minimized ? 0 : 1);

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
