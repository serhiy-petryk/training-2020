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
                Debug.Print(
                    $"DatePickerEffect.ClearButton is not implemented for {d.GetType().Namespace}.{d.GetType().Name} type");
                return;
            }

            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                RemoveClearButton(dp);

                if ((bool)e.NewValue && !dp.IsElementDisposing())
                    AddClearButton(dp);
            }, DispatcherPriority.Loaded);
        }

        private static void AddClearButton(DatePicker dp)
        {
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
            var clearButton = Tips.GetVisualChildren(dp).FirstOrDefault(btn => btn is Button && ((Button)btn).Name == ClearButtonName) as Button;
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

            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                dp.SelectedDateChanged -= OnSelectedDateChanged;
                var dpdStart = DependencyPropertyDescriptor.FromProperty(Calendar.DisplayDateStartProperty, typeof(DatePicker));
                dpdStart.RemoveValueChanged(dp, OnChangeDateLimit);
                var dpdEnd = DependencyPropertyDescriptor.FromProperty(Calendar.DisplayDateEndProperty, typeof(DatePicker));
                dpdEnd.RemoveValueChanged(dp, OnChangeDateLimit);

                //if (e.NewValue == null)
                    //return;

                if (!dp.IsElementDisposing())
                {
                    dp.SelectedDateChanged += OnSelectedDateChanged;
                    dpdStart.AddValueChanged(dp, OnChangeDateLimit);
                    dpdEnd.AddValueChanged(dp, OnChangeDateLimit);

                    CheckDatePicker(dp);
                }
            }, DispatcherPriority.Loaded);
        }

        private static void OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckDatePicker(sender as DatePicker);
        }

        private static void OnChangeDateLimit(object sender, EventArgs e)
        {
            CheckDatePicker(sender as DatePicker);
        }

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
            if (!(d is FrameworkElement fe))
            {
                Debug.Print($"DatePickerEffect.HideInnerBorders is not implemented for {d.GetType().Namespace}.{d.GetType().Name} type");
                return;
            }

            if (e.NewValue is bool toHide)
                ControlHelper.HideInnerBorderOfDatePickerTextBox(fe, toHide);
        }
        #endregion
    }
}
