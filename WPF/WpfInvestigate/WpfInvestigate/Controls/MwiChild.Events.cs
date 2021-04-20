using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using WpfInvestigate.Helpers;
using WpfInvestigate.ViewModels;

namespace WpfInvestigate.Controls
{
    public partial class MwiChild
    {
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Loaded -= OnLoaded; // run only on startup mwichild
            AddLoadedEvents();
            if (!IsWindowed)
                AnimateShow();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (this.IsElementDisposing())
            {
                // Debug.Print($"MwiChild. Unloaded: {this.IsElementDisposing()}, {_controlId}");
                Loaded -= OnLoaded;
                Unloaded -= OnUnloaded;
                var dpdBackground = DependencyPropertyDescriptor.FromProperty(BackgroundProperty, typeof(MwiChild));
                dpdBackground.RemoveValueChanged(this, OnBackgroundChanged);
                var dpdScaleValue = DependencyPropertyDescriptor.FromProperty(MwiAppViewModel.ScaleValueProperty, typeof(MwiAppViewModel));
                dpdScaleValue.RemoveValueChanged(MwiAppViewModel.Instance, OnScaleValueChanged);

                MwiAppViewModel.Instance.PropertyChanged -= OnMwiAppViewModelPropertyChanged;

                AddLoadedEvents(true);
                AddVisualParentChangedEvents(true);

                /*var elements = (new[] { this }).Union(this.GetVisualChildren()).ToArray();
                foreach (var element in elements)
                {
                    if (element is UIElement uiElement)
                        Events.RemoveAllRoutedEventHandlers(uiElement);
                    // Events.RemoveAllEventSubsriptions(element);
                }*/
                this.CleanDependencyObject();
            }
        }

        private void AddLoadedEvents(bool onlyRemove = false)
        {
            if (MovingThumb != null)
                MovingThumb.MouseDoubleClick -= OnMovingThumbMouseDoubleClick;

            if (onlyRemove)
            {
                MovingThumb = null;
                return;
            }

            if (GetTemplateChild("MovingThumb") is Thumb movingThumb)
            {
                if (MovingThumb != movingThumb)
                    MovingThumb = movingThumb;
                MovingThumb.MouseDoubleClick += OnMovingThumbMouseDoubleClick;
            }
            else if (MovingThumb != null)
                MovingThumb = null;

            void OnMovingThumbMouseDoubleClick(object sender, MouseButtonEventArgs e)
            {
                var element = (FrameworkElement)sender;
                element.IsEnabled = false;
                ToggleMaximize(null);
                Dispatcher.InvokeAsync(new Action(() => element.IsEnabled = true), DispatcherPriority.Render); // nexttick action to prevent MoveThumb_OnDragDelta event
            }
        }

        private async void AddVisualParentChangedEvents(bool onlyRemove = false)
        {
            if (_activatedHost != null)
            {
                _activatedHost.Activated -= OnWindowActivated;
                _activatedHost.Deactivated -= OnWindowDeactivated;
                _activatedHost = null;
                return;
            }

            if (!IsWindowed || onlyRemove) return;

            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Normal).Task;
            _activatedHost = Parent as Window;
            _activatedHost.Icon = (GetTemplateChild("IconImage") as Image)?.Source;

            if (!string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(_activatedHost.Title))
                _activatedHost.Title = Title;
            else if (string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(_activatedHost.Title))
                Title = _activatedHost.Title;

            OnHostClosed(_activatedHost, null);
            _activatedHost.KeyDown += OnHostKeyDown;
            _activatedHost.Closed += OnHostClosed;
            _activatedHost.Activated += OnWindowActivated;
            _activatedHost.Deactivated += OnWindowDeactivated;

            void OnHostKeyDown(object sender, KeyEventArgs e)
            {
                if (Keyboard.Modifiers == ModifierKeys.Alt && Keyboard.IsKeyDown(Key.F4)) // Is Alt+F4 key pressed
                {
                    CmdClose.Execute(null);
                    e.Handled = true;
                }
            }
            void OnHostClosed(object sender, EventArgs e)
            {
                if (sender is Window wnd)
                {
                    wnd.Closed -= OnHostClosed;
                    wnd.KeyDown -= OnHostKeyDown;
                    wnd.Activated -= OnWindowActivated;
                    wnd.Deactivated -= OnWindowDeactivated;
                }
            }
            void OnWindowActivated(object sender, EventArgs e) => Activate();
            void OnWindowDeactivated(object sender, EventArgs e) => IsActive = false;
        }

    }
}
