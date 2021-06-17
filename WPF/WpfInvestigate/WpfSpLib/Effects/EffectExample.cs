using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace WpfSpLib.Effects
{
    public class EffectExample
    {
        private static readonly ConcurrentDictionary<FrameworkElement, object> _activated = new ConcurrentDictionary<FrameworkElement, object>();

        private static void OnAttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control)
            {
                control.IsVisibleChanged -= Element_IsVisibleChanged;
                control.IsVisibleChanged += Element_IsVisibleChanged;

                if (control.IsVisible)
                {
                    if (_activated.TryAdd(control, null)) Activate(control);
                    PropertyChanged(control, e);
                }
                else
                {
                    if (_activated.TryRemove(control, out var o)) Deactivate(control);
                }
            }
            else
                Debug.Print($"Effect is not implemented for {d.GetType().Namespace}.{d.GetType().Name} type");

            void Element_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e2) =>
                OnAttachedPropertyChanged((Control)sender, new DependencyPropertyChangedEventArgs(e2.Property, null, null));
        }

        private static void Activate(Control control) { }
        private static void Deactivate(Control control) { }
        private static void PropertyChanged(Control control, DependencyPropertyChangedEventArgs e) { }
    }
}
