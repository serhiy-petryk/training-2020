using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WpfSpLib.Common;
using WpfSpLib.Helpers;

namespace WpfSpLib.Effects
{
    /// <summary>
    /// DatePickerEffect.IsNullable property: supports IsNullable and DisplayDateStart/End (as Minimum/Maximum dates) for SelectedDate of DatePicker
    /// DatePickerEffect.ClearButton property: hide/show clear button for DatePicker
    /// DatePickerEffect.HideInnerBorders property: hide/show borders of DatePickerTextBox which is child of any framework element
    /// Usage:  <DatePicker controls:DatePickerEffect.ClearButton="True" controls:DatePickerEffect.IsNullable="True" />
    /// </summary>
    public class DatePickerEffect
    {
        #region ===============  ClearButton attached property  ================
        private const string ClearButtonName = "DatePickerEffectClearButton";

        public static readonly DependencyProperty ClearButtonProperty = DependencyProperty.RegisterAttached("ClearButton",
            typeof(bool), typeof(DatePickerEffect), new PropertyMetadata(false, propertyChangedCallback: OnClearButtonChanged));
        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
        public static void SetClearButton(DependencyObject d, bool value) => d.SetValue(ClearButtonProperty, value);
        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
        public static bool GetClearButton(DependencyObject d) => (bool)d.GetValue(ClearButtonProperty);

        private static void OnClearButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DatePicker dp))
            {
                Debug.Print($"DatePickerEffect.ClearButton is not implemented for {d.GetType().Namespace}.{d.GetType().Name} type");
                return;
            }

            dp.Unloaded -= Deactivate_ClearButton;
            dp.Loaded -= Activate_ClearButton;

            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                dp.Unloaded += Deactivate_ClearButton;
                dp.Loaded += Activate_ClearButton;
                Activate_ClearButton(dp, null);
            }, DispatcherPriority.Loaded);
        }

        private static void Activate_ClearButton(object sender, RoutedEventArgs e)
        {
            Deactivate_ClearButton(sender, null);
            var dp = (DatePicker) sender;
            AddClearButton(dp);
        }

        private static void Deactivate_ClearButton(object sender, RoutedEventArgs e)
        {
            var dp = (DatePicker)sender;
            RemoveClearButton(dp);
        }

        private static void AddClearButton(DatePicker dp)
        {
            if (!dp.IsVisible) return;
            if (dp.GetVisualChildren().OfType<Button>().FirstOrDefault(btn => btn.Name == ClearButtonName) != null)
                return;

            var button = dp.Template.FindName("PART_Button", dp) as Button;
            var grid = button.Parent as Grid;
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var style = dp.FindResource("ClearBichromeButtonStyle") as Style;
            var clearButton = new Button
            {
                Name = ClearButtonName, Style = style, Width = 18, 
                Margin = new Thickness(-2, 0, 1 - dp.Padding.Right, 0),
                Padding = new Thickness(3), Focusable = false
            };

            if (dp.Background == null || dp.Background == Brushes.Transparent)
                dp.Background = Tips.GetActualBackgroundBrush(dp);

            clearButton.Click += ClearButton_Click;
            grid.Children.Add(clearButton);
            Grid.SetColumn(clearButton, grid.ColumnDefinitions.Count - 1);
        }

        private static void RemoveClearButton(DatePicker dp)
        {
            var clearButton = dp.GetVisualChildren().OfType<Button>().FirstOrDefault(btn => btn.Name == ClearButtonName);
            if (clearButton != null)
            {
                clearButton.Click -= ClearButton_Click;
                var grid = VisualTreeHelper.GetParent(clearButton) as Grid;
                grid?.Children.Remove(clearButton);
            }
        }

        private static void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            var current = (DependencyObject)sender;
            while (current != null && !(current is DatePicker))
                current = VisualTreeHelper.GetParent(current) ?? (current as FrameworkElement)?.Parent;

            if (current != null)
            {
                var isNullable = GetIsNullable(current) ?? false;
                ((DatePicker)current).SelectedDate = isNullable ? (DateTime?)null : DateTime.Today;
                ((DatePicker)current).Focus();
            }
        }
        #endregion

        #region ===============  IsNullable attached property  ================
        public static readonly DependencyProperty IsNullableProperty = DependencyProperty.RegisterAttached("IsNullable",
            typeof(bool?), typeof(DatePickerEffect), new PropertyMetadata(null, propertyChangedCallback: OnIsNullableChanged));
        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
        public static void SetIsNullable(DependencyObject d, bool? value) => d.SetValue(IsNullableProperty, value);
        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
        public static bool? GetIsNullable(DependencyObject d) => (bool?)d.GetValue(IsNullableProperty);

        private static void OnIsNullableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DatePicker dp))
            {
                Debug.Print($"DatePickerEffect.IsNullable is not implemented for {d.GetType().Namespace}.{d.GetType().Name} type");
                return;
            }

            dp.Unloaded -= Deactivate_IsNullable;
            dp.Loaded -= Activate_IsNullable;

            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                dp.Unloaded += Deactivate_IsNullable;
                dp.Loaded += Activate_IsNullable;
                Activate_IsNullable(dp, null);
            }, DispatcherPriority.Loaded);
        }

        private static void Activate_IsNullable(object sender, RoutedEventArgs e)
        {
            Deactivate_IsNullable(sender, e);
            var dp = (DatePicker)sender;
            dp.SelectedDateChanged += OnSelectedDateChanged;
            var dpdStart = DependencyPropertyDescriptor.FromProperty(Calendar.DisplayDateStartProperty, typeof(DatePicker));
            dpdStart.AddValueChanged(dp, OnChangeDateLimit);
            var dpdEnd = DependencyPropertyDescriptor.FromProperty(Calendar.DisplayDateEndProperty, typeof(DatePicker));
            dpdEnd.AddValueChanged(dp, OnChangeDateLimit);
            CheckDatePicker(dp);
        }

        private static void Deactivate_IsNullable(object sender, RoutedEventArgs e)
        {
            var dp = (DatePicker)sender;
            dp.SelectedDateChanged -= OnSelectedDateChanged;
            var dpdStart = DependencyPropertyDescriptor.FromProperty(Calendar.DisplayDateStartProperty, typeof(DatePicker));
            dpdStart.RemoveValueChanged(dp, OnChangeDateLimit);
            var dpdEnd = DependencyPropertyDescriptor.FromProperty(Calendar.DisplayDateEndProperty, typeof(DatePicker));
            dpdEnd.RemoveValueChanged(dp, OnChangeDateLimit);
        }

        private static void OnSelectedDateChanged(object sender, SelectionChangedEventArgs e) => CheckDatePicker(sender as DatePicker);
        private static void OnChangeDateLimit(object sender, EventArgs e) => CheckDatePicker(sender as DatePicker);

        private static void CheckDatePicker(DatePicker dp)
        {
            var isNullable = GetIsNullable(dp) ?? false;
            if (!dp.SelectedDate.HasValue && isNullable)
                return;

            var value = dp.SelectedDate ?? DateTime.Today;
            if (dp.DisplayDateStart.HasValue && dp.DisplayDateStart.Value > value)
                value = dp.DisplayDateStart.Value;
            else if (dp.DisplayDateEnd.HasValue && dp.DisplayDateEnd.Value < value)
                value = dp.DisplayDateEnd.Value;

            if (dp.SelectedDate != value)
                dp.SelectedDate = value;
        }
        #endregion

        #region ===============  Hide inner borders  ================
        public static readonly DependencyProperty HideInnerBordersProperty = DependencyProperty.RegisterAttached("HideInnerBorders",
            typeof(bool), typeof(DatePickerEffect), new PropertyMetadata(false, propertyChangedCallback: OnHideInnerBordersChanged));
        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
        public static void SetHideInnerBorders(DependencyObject d, bool value) => d.SetValue(HideInnerBordersProperty, value);
        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
        public static bool GetHideInnerBorders(DependencyObject d) => (bool)d.GetValue(HideInnerBordersProperty);

        private static void OnHideInnerBordersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FrameworkElement element))
            {
                Debug.Print($"DatePickerEffect.HideInnerBorders is not implemented for {d.GetType().Namespace}.{d.GetType().Name} type");
                return;
            }

            element.Loaded -= Activate_HideInnerBorder;

            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                element.Loaded += Activate_HideInnerBorder;
                Activate_HideInnerBorder(element, null);
            }, DispatcherPriority.Loaded);
        }

        private static void Activate_HideInnerBorder(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement) sender;
            var toHide = GetHideInnerBorders(element);
            ControlHelper.HideInnerBorderOfDatePickerTextBox(element, toHide);
        }
        #endregion
    }
}
