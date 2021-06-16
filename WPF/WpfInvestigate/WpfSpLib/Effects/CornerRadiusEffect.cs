using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using WpfSpLib.Common;
using WpfSpLib.Helpers;

namespace WpfSpLib.Effects
{
    /// <summary>
    /// </summary>
    public class CornerRadiusEffect
    {
        #region ===========  OnPropertyChanged  ===========

        private static void OnAttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                element.IsVisibleChanged -= Element_IsVisibleChanged;
                element.IsVisibleChanged += Element_IsVisibleChanged;

                if (element.IsVisible)
                    PropertyChangeRouter(element, e);
            }
            else
                Debug.Print($"CornerRadiusEffect is not implemented for {d.GetType().Namespace}.{d.GetType().Name} type");

            void Element_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e2) =>
                PropertyChangeRouter((FrameworkElement) sender, new DependencyPropertyChangedEventArgs(e2.Property, null, null));
        }

        private static void PropertyChangeRouter(FrameworkElement element, DependencyPropertyChangedEventArgs e)
        {
            if (element.IsVisible)
            {
                if (e.Property == UIElement.IsVisibleProperty)
                    Activate(element);
                if (e.Property == CornerRadiusProperty)
                    UpdateBorders(element, null);
            }
            else if (e.Property == UIElement.IsVisibleProperty)
                Deactivate(element);
        }
        #endregion

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
            "CornerRadius", typeof(CornerRadius), typeof(CornerRadiusEffect), new FrameworkPropertyMetadata(new CornerRadius(), OnAttachedPropertyChanged));
        public static CornerRadius GetCornerRadius(DependencyObject obj) => (CornerRadius)obj.GetValue(CornerRadiusProperty);
        public static void SetCornerRadius(DependencyObject obj, CornerRadius value) => obj.SetValue(CornerRadiusProperty, value);
        private static void Activate(FrameworkElement element)
        {
            Deactivate(element);
            var dpd = DependencyPropertyDescriptor.FromProperty(Border.BorderThicknessProperty, typeof(Border));
            dpd.AddValueChanged(element, UpdateBorders);
            element.SizeChanged += UpdateBorders;
            UpdateBorders(element, null);
        }

        private static void Deactivate(FrameworkElement element)
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(Border.BorderThicknessProperty, typeof(Border));
            dpd.RemoveValueChanged(element, UpdateBorders);
            element.SizeChanged -= UpdateBorders;
        }

        private static void UpdateBorders(object sender, EventArgs e)
        {
            if (!(sender is FrameworkElement element && element.IsVisible)) return;
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
