using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// DatePickerExtension.IsNullable property: supports IsNullable and DisplayDateStart/End (as Minimum/Maximum dates) for SelectedDate of DatePicker
    /// DatePickerExtension.ClearButton property: hide/show clear button for DatePicker
    /// Usage:  <DatePicker controls:DatePickerExtension.ClearButton="True" controls:DatePickerExtension.IsNullable="True" />
    /// </summary>
    public class DatePickerExtension
    {
        #region ===============  ClearButton attached property  ================
        private const string ClearButtonName = "DatePickerExtensionClearButton";

        public static readonly DependencyProperty ClearButtonProperty = DependencyProperty.RegisterAttached("ClearButton",
            typeof(bool), typeof(DatePickerExtension), new PropertyMetadata(false, propertyChangedCallback: OnClearButtonChanged));

        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
        public static void SetClearButton(DependencyObject d, bool value) => d.SetValue(ClearButtonProperty, value);
        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
        public static bool GetClearButton(DependencyObject d) => (bool)d.GetValue(ClearButtonProperty);

        private static void OnClearButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DatePicker dp))
            {
                Debug.Print($"DatePickerExtension.ClearButton is not implemented for {d.GetType().Namespace}.{d.GetType().Name} type");
                return;
            }

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                SetClearButton(d, (bool)e.NewValue);
                OnUnloadedForClearButton(dp, null);

                dp.Unloaded += OnUnloadedForClearButton;

                if ((bool)e.NewValue)
                    AddClearButton(dp);
            }));
        }

        private static void OnUnloadedForClearButton(object sender, RoutedEventArgs e)
        {
            var dp = sender as DatePicker;
            dp.Unloaded -= OnUnloadedForClearButton;
            RemoveClearButton(dp);
        }


        private static void AddClearButton(DatePicker dp)
        {
            var button = Tips.GetVisualChildren(dp).FirstOrDefault(btn => btn is Button && ((Button)btn).Name == ClearButtonName) as Button;
            if (button !=null)
                return;

            button = dp.Template.FindName("PART_Button", dp) as Button;
            var grid = button.Parent as Grid;
            grid.ColumnDefinitions.Add(new ColumnDefinition {Width = GridLength.Auto});
            var style = dp.FindResource("ClearAnimatedButtonStyle") as Style;
            var clearButton = new Button
            {
                Name = ClearButtonName, Style = style, Width = 18, Margin = new Thickness(-2, 0, 0, 0),
                Padding = new Thickness(3)
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
            }
        }
        #endregion

        #region ===============  IsNullable attached property  ================
        public static readonly DependencyProperty IsNullableProperty = DependencyProperty.RegisterAttached("IsNullable",
            typeof(bool?), typeof(DatePickerExtension), new PropertyMetadata(null, propertyChangedCallback: OnIsNullableChanged));
        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
        public static void SetIsNullable(DependencyObject d, bool? value) => d.SetValue(IsNullableProperty, value);
        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
        public static bool? GetIsNullable(DependencyObject d) => (bool?)d.GetValue(IsNullableProperty);

        private static void OnIsNullableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DatePicker dp))
            {
                Debug.Print($"DatePickerExtension.IsNullable is not implemented for {d.GetType().Namespace}.{d.GetType().Name} type");
                return;
            }

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                OnUnloadedForIsNullable(dp, null);
                SetIsNullable(d, (bool?)e.NewValue);
                if (e.NewValue == null)
                    return;

                var dpdStart = DependencyPropertyDescriptor.FromProperty(Calendar.DisplayDateStartProperty, typeof(DatePicker));
                dpdStart.AddValueChanged(dp, OnChangeDateLimit);
                var dpdEnd = DependencyPropertyDescriptor.FromProperty(Calendar.DisplayDateEndProperty, typeof(DatePicker));
                dpdEnd.AddValueChanged(dp, OnChangeDateLimit);

                dp.SelectedDateChanged += OnSelectedDateChanged;
                dp.Unloaded += OnUnloadedForIsNullable;

                CheckDatePicker(dp);
            }));
        }

        private static void OnUnloadedForIsNullable(object sender, RoutedEventArgs e)
        {
            var dp = sender as DatePicker;
            dp.SelectedDateChanged -= OnSelectedDateChanged;
            dp.Unloaded -= OnUnloadedForIsNullable;
            var dpdStart = DependencyPropertyDescriptor.FromProperty(Calendar.DisplayDateStartProperty, typeof(DatePicker));
            dpdStart.RemoveValueChanged(dp, OnChangeDateLimit);
            var dpdEnd = DependencyPropertyDescriptor.FromProperty(Calendar.DisplayDateEndProperty, typeof(DatePicker));
            dpdEnd.RemoveValueChanged(dp, OnChangeDateLimit);
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
    }
}
