﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WpfSpLib.Common;
using WpfSpLib.Helpers;

namespace WpfSpLib.Effects
{
    /// <summary>
    /// </summary>
    public class CornerRadiusEffect
    {
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
            "CornerRadius", typeof(CornerRadius), typeof(CornerRadiusEffect), new FrameworkPropertyMetadata(new CornerRadius(), OnCornerRadiusChanged));
        public static CornerRadius GetCornerRadius(DependencyObject obj) => (CornerRadius)obj.GetValue(CornerRadiusProperty);
        public static void SetCornerRadius(DependencyObject obj, CornerRadius value) => obj.SetValue(CornerRadiusProperty, value);
        private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
                element.AttachedPropertyChangedHandler(Activate, Deactivate, DispatcherPriority.Background);
        }

        private static void Activate(object sender, RoutedEventArgs e)
        {
            Deactivate(sender, e);
            var element = (FrameworkElement)sender;
            var dpd = DependencyPropertyDescriptor.FromProperty(Border.BorderThicknessProperty, typeof(Border));
            dpd.AddValueChanged(element, UpdateBorders);
            element.SizeChanged += UpdateBorders;
            if (element.IsVisible)
                UpdateBorders(element, null);
        }

        private static void Deactivate(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            var dpd = DependencyPropertyDescriptor.FromProperty(Border.BorderThicknessProperty, typeof(Border));
            dpd.RemoveValueChanged(element, UpdateBorders);
            element.SizeChanged -= UpdateBorders;
        }

        private static void UpdateBorders(object sender, EventArgs e)
        {
            if (!(sender is FrameworkElement element && element.IsLoaded)) return;
            var newRadius = GetCornerRadius(element);
            foreach (var border in ControlHelper.GetMainElements<Border>(element))
            {
                var borderSize = border.BorderThickness;
                var radius = new CornerRadius(
                    GetCornerWidth(newRadius.TopLeft, borderSize.Top, borderSize.Left),
                    GetCornerWidth(newRadius.TopRight, borderSize.Top, borderSize.Right),
                    GetCornerWidth(newRadius.BottomRight, borderSize.Bottom, borderSize.Right),
                    GetCornerWidth(newRadius.BottomLeft, borderSize.Bottom, borderSize.Left));

                border.CornerRadius = radius;
                ControlHelper.ClipToBoundBorder(border);
            }
        }

        private static double GetCornerWidth(double requiredCornerWidth, double firstBorderWidth, double secondBorderWidth)
        {
            if (Tips.AreEqual(requiredCornerWidth, 0.0)) return 0.0;
            var cornerWidth = requiredCornerWidth - (firstBorderWidth + secondBorderWidth) / 4;
            return Math.Max(Tips.SCREEN_TOLERANCE, cornerWidth);
        }
    }
}
