﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WpfInvestigate.Common;
using WpfInvestigate.Controls;

namespace WpfInvestigate.Effects
{
    public class FocusVisualEffect
    {
        #region ==============  Properties  ==============
        public static readonly DependencyProperty AlwaysShowFocusProperty = DependencyProperty.RegisterAttached(
            "AlwaysShowFocus", typeof(bool), typeof(FocusVisualEffect), new UIPropertyMetadata(false));
        public static bool GetAlwaysShowFocus(DependencyObject obj) => (bool)obj.GetValue(AlwaysShowFocusProperty);
        public static void SetAlwaysShowFocus(DependencyObject obj, bool value) => obj.SetValue(AlwaysShowFocusProperty, value);
        //================
        public static readonly DependencyProperty FocusControlStyleProperty = DependencyProperty.RegisterAttached(
            "FocusControlStyle", typeof(Style), typeof(FocusVisualEffect), new UIPropertyMetadata(null, OnFocusControlStyleChanged));
        public static Style GetFocusControlStyle(DependencyObject obj) => (Style)obj.GetValue(FocusControlStyleProperty);
        public static void SetFocusControlStyle(DependencyObject obj, Style value) => obj.SetValue(FocusControlStyleProperty, value);

        private static void OnFocusControlStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                element.SizeChanged -= Element_ChangeFocus;
                var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsKeyboardFocusWithinProperty, typeof(UIElement));
                dpd.RemoveValueChanged(element, OnElementFocusChanged);

                if (e.NewValue is Style)
                {
                    if (element.FocusVisualStyle != null)
                        element.FocusVisualStyle = null;

                    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                    {
                        element.SizeChanged += Element_ChangeFocus;
                        dpd.AddValueChanged(element, OnElementFocusChanged);
                    }));
                }
            }
        }
        #endregion
        private static void OnElementFocusChanged(object sender, EventArgs e) => Element_ChangeFocus(sender, null);

        private static void Element_ChangeFocus(object sender, SizeChangedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            var isFocused = element.IsKeyboardFocusWithin;

            if (isFocused)
            {
                if (!GetAlwaysShowFocus(element))
                {
                    var a1 = element.Resources["FocusEffect_FocusTrack"] as bool?;
                    var isKeyboardMostRecentInputDevice = a1 ?? IsKeyboardMostRecentInputDevice();
                    if (a1 == null)
                        element.Resources.Add("FocusEffect_FocusTrack", isKeyboardMostRecentInputDevice);
                    if (!isKeyboardMostRecentInputDevice)
                        return;
                }

                var layer = AdornerLayer.GetAdornerLayer(element);
                var adornerControl = layer.GetAdorners(element)?.OfType<AdornerControl>().FirstOrDefault(a => a.Child.Name == "FocusControl");
                if (adornerControl == null)
                {
                    var control = new Control
                    {
                        Name = "FocusControl", Focusable = false, IsHitTestVisible = false,
                        Style =  GetFocusControlStyle(element)
                    };

                    adornerControl = new AdornerControl(element) {Child = control, AdornerSize = AdornerControl.AdornerSizeType.AdornedElement, Opacity = 0.0};
                    layer.Add(adornerControl);
                }

                var opacityAnimation = new DoubleAnimation { Duration = AnimationHelper.AnimationDurationSlow };
                opacityAnimation.SetFromToValues(adornerControl.Opacity, 1.0);
                adornerControl.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            }
            else
            {
                if (!GetAlwaysShowFocus(element))
                    element.Resources.Remove("FocusEffect_FocusTrack");
                var layer = AdornerLayer.GetAdornerLayer(element);
                var adorners = layer?.GetAdorners(element) ?? new Adorner[0];
                foreach (var adorner in adorners.OfType<AdornerControl>().Where(a => a.Child.Name == "FocusControl"))
                {
                    var opacityAnimation = new DoubleAnimation {Duration = AnimationHelper.AnimationDurationSlow};
                    opacityAnimation.SetFromToValues(adorner.Opacity, 0.0);
                    adorner.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
                }

                // if isFocused=false не завжди спрацьовує фокус на новому елементі -> Activate focus on focused element
                var focusedControl = Keyboard.FocusedElement as FrameworkElement;
                if (focusedControl != null && focusedControl != element && GetFocusControlStyle(focusedControl) != null)
                    Element_ChangeFocus(focusedControl, null);
            }
        }

        private static bool IsKeyboardMostRecentInputDevice() => InputManager.Current.MostRecentInputDevice is KeyboardDevice;
    }
}