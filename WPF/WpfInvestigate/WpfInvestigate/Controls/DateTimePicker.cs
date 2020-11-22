using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace WpfInvestigate.Controls
{
    /// <summary>
    ///     Represents a control that allows the user to select a date and a time.
    /// </summary>
    [TemplatePart(Name = ElementCalendar, Type = typeof(Calendar))]
    [DefaultEvent("SelectedDateChanged")]
    public class DateTimePicker : TimePickerBase
    {
        public static readonly RoutedEvent SelectedDateChangedEvent = EventManager.RegisterRoutedEvent("SelectedDateChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<DateTime?>), typeof(DateTimePicker));

        public static readonly DependencyProperty SelectedDateProperty = DatePicker.SelectedDateProperty.AddOwner(typeof(DateTimePicker),
            new FrameworkPropertyMetadata(default(DateTime?), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateChanged, CoerceDateTime));
        public static readonly DependencyProperty DisplayDateStartProperty = DatePicker.DisplayDateStartProperty.AddOwner(typeof(DateTimePicker));
        public static readonly DependencyProperty DisplayDateEndProperty = DatePicker.DisplayDateEndProperty.AddOwner(typeof(DateTimePicker));
        public static readonly DependencyProperty IsTodayHighlightedProperty = DatePicker.IsTodayHighlightedProperty.AddOwner(typeof(DateTimePicker));
        public static readonly DependencyProperty SelectedDateFormatProperty = DatePicker.SelectedDateFormatProperty.AddOwner(
            typeof(DateTimePicker), new FrameworkPropertyMetadata(DatePickerFormat.Short, OnSelectedDateFormatChanged));

        public static readonly DependencyProperty DatePickerModeProperty = DependencyProperty.Register("DatePickerMode", typeof(bool),
            typeof(DateTimePicker), new PropertyMetadata(false, OnDatePickerModeChanged));

        private const string ElementCalendar = "PART_Calendar";
        private Calendar _calendar;
        private bool _isDateChanging;
        private DateTime _defaultDate => DisplayDateStart.HasValue && DisplayDateStart.Value > DateTime.Today ? DisplayDateStart.Value : DateTime.Today;

        static DateTimePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DateTimePicker), new FrameworkPropertyMetadata(typeof(DateTimePicker)));
        }

        /// <summary>
        ///     Occurs when the <see cref="SelectedDate" /> property is changed.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<DateTime?> SelectedDateChanged
        {
            add => AddHandler(SelectedDateChangedEvent, value);
            remove => RemoveHandler(SelectedDateChangedEvent, value);
        }

        /// <summary>
        ///     Gets or sets the currently selected date.
        /// </summary>
        /// <returns>
        ///     The date currently selected. The default is null.
        /// </returns>
        public DateTime? SelectedDate
        {
            get => (DateTime?)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        /// <summary>
        ///     Gets or sets the first date to be displayed.
        /// </summary>
        /// <returns>The first date to display.</returns>
        public DateTime? DisplayDateStart
        {
            get => (DateTime?)GetValue(DisplayDateStartProperty);
            set => SetValue(DisplayDateStartProperty, value);
        }

        /// <summary>
        ///     Gets or sets the last date to be displayed.
        /// </summary>
        /// <returns>The last date to display.</returns>
        public DateTime? DisplayDateEnd
        {
            get => (DateTime?)GetValue(DisplayDateEndProperty);
            set => SetValue(DisplayDateEndProperty, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether the current date will be highlighted.
        /// </summary>
        /// <returns>true if the current date is highlighted; otherwise, false. The default is true. </returns>
        public bool IsTodayHighlighted
        {
            get => (bool)GetValue(IsTodayHighlightedProperty);
            set => SetValue(IsTodayHighlightedProperty, value);
        }

        /// <summary>
        /// Gets or sets the format that is used to display the selected date.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(DatePickerFormat.Short)]
        public DatePickerFormat SelectedDateFormat
        {
            get => (DatePickerFormat)GetValue(SelectedDateFormatProperty);
            set => SetValue(SelectedDateFormatProperty, value);
        }

        /// <summary>
        ///     Does picker support only date?
        /// </summary>
        public bool DatePickerMode
        {
            get => (bool)GetValue(DatePickerModeProperty);
            set => SetValue(DatePickerModeProperty, value);
        }

        public override void OnApplyTemplate()
        {
            _calendar = GetTemplateChild(ElementCalendar) as Calendar;
            ApplyBindings();
            base.OnApplyTemplate();
            CoerceValue(SelectedDateProperty);
            OnDatePickerModeChanged(this, new DependencyPropertyChangedEventArgs(DatePickerModeProperty, null, DatePickerMode));
        }

        private void ApplyBindings()
        {
            if (_calendar != null)
            {
                _calendar.SetBinding(Calendar.SelectedDateProperty, GetBinding(SelectedDateProperty));
                _calendar.SetBinding(Calendar.DisplayDateStartProperty, GetBinding(DisplayDateStartProperty));
                _calendar.SetBinding(Calendar.DisplayDateEndProperty, GetBinding(DisplayDateEndProperty));
                _calendar.SetBinding(Calendar.IsTodayHighlightedProperty, GetBinding(IsTodayHighlightedProperty));
            }
        }
        private Binding GetBinding(DependencyProperty property) => new Binding(property.Name) { Source = this };

        protected virtual void OnSelectedDateChanged(RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            RaiseEvent(e);
        }

        private static void OnSelectedDateFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = d as DateTimePicker;
            picker?.WriteValueToTextBox();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (double.IsNaN(Width))
                Width = DatePickerMode ? 110 : 150;
        }

        protected override void OnPopupOpened(object sender, EventArgs e)
        {
            SetDatePartValues();
            base.OnPopupOpened(sender, e);
        }

        protected sealed override void ApplyCulture()
        {
            base.ApplyCulture();
            if (_calendar != null)
            {
                _calendar.FirstDayOfWeek = Culture.DateTimeFormat.FirstDayOfWeek;
                // Refresh month/week names of calendar
                var mi = _calendar.GetType().GetMethod("UpdateCellItems", BindingFlags.Instance | BindingFlags.NonPublic);
                mi?.Invoke(_calendar, null);
            }
        }

        protected override string GetValueForTextBox()
        {
            var formatInfo = Culture.DateTimeFormat;
            var dateFormat = SelectedDateFormat == DatePickerFormat.Long ? formatInfo.LongDatePattern : formatInfo.ShortDatePattern;

            var valueForTextBox = SelectedDate?.ToString(dateFormat, Culture);
            return valueForTextBox + (!DatePickerMode && SelectedDate.HasValue ? " " + base.GetValueForTextBox() : null);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);
            if (Mouse.Captured is CalendarItem) 
                Mouse.Capture(null);
        }

        protected override void OnBaseValueChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            base.OnBaseValueChanged(sender, e);
            SetDatePartValues();
        }

        protected override void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            var textBoxValue = ((DatePickerTextBox)sender).Text;
            DateTime? dt = null;
            if (!string.IsNullOrWhiteSpace(textBoxValue))
            {
                if (DateTime.TryParse(textBoxValue, Culture, System.Globalization.DateTimeStyles.None, out var tempDt))
                    dt = tempDt;
            }

            SelectedDate = dt;
            WriteValueToTextBox(); // need in case: isNullbale=false; set textbox=empty twice (no OnSelectedDateChanged event for second call and textbox is empty but SelectedDate!=empty)
        }

        private static void OnDatePickerModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (DateTimePicker)d;
            if (picker.GetTemplateChild("PART_Clock") is FrameworkElement clock)
            {
                clock.Visibility = (bool) e.NewValue ? Visibility.Collapsed : Visibility.Visible;
                OnSelectedDateChanged(d, new DependencyPropertyChangedEventArgs(SelectedDateProperty, null, picker.SelectedDate));
            }
        }

        private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (DateTimePicker)d;
            if (picker._isDateChanging)
                return;

            /* Without deactivating changing SelectedTime would callbase.OnSelectedTimeChanged.
             * This would write too and this would result in duplicate writing.
             * More problematic would be instead that a short amount of time SelectedTime would be as value in TextBox
             */
            picker._isDateChanging = true;
            var dt = (DateTime?)e.NewValue;
            picker.SelectedDate = dt;
            picker.SelectedTime = dt?.TimeOfDay;
            picker.OnSelectedDateChanged(new RoutedPropertyChangedEventArgs<DateTime?>((DateTime?)e.OldValue, (DateTime?)e.NewValue, SelectedDateChangedEvent));

            picker._isDateChanging = false;
            picker.WriteValueToTextBox();

            if (picker.DatePickerMode)
                picker.ClosePopup();
        }

        private static object CoerceDateTime(DependencyObject d, object basevalue)
        {
            var picker = (DateTimePicker)d;
            var dt = (DateTime?)basevalue;
            if (dt == null)
                return picker.IsNullable ? (DateTime?)null : picker._defaultDate;

            if (!picker.DatePickerMode && picker._popup != null && picker._popup.IsOpen && dt.Value.TimeOfDay == TimeSpan.Zero) // Calendar is changing
                dt = dt.Value + picker.SelectedTime;
            else if (picker.DatePickerMode) dt = dt.Value.Date; // OnlyDate mode

            if (picker.DisplayDateStart.HasValue && picker.DisplayDateStart.Value > dt.Value)
                return picker.DisplayDateStart.Value;
            if (picker.DisplayDateEnd.HasValue && picker.DisplayDateEnd.Value < dt.Value)
                return picker.DisplayDateEnd.Value;

            return dt.Value;
        }

        private void SetDatePartValues()
        {
            if (_isDateChanging)
                return;

            _isDateChanging = true;
            if (_calendar != null)
            {
                _calendar.SelectedDate = SelectedDate;
                _calendar.DisplayDate = SelectedDate ?? _defaultDate;
            }
            _isDateChanging = false;
        }

        protected override void IsNullableChanged()
        {
            base.IsNullableChanged();
            if (!IsNullable && !SelectedDate.HasValue)
                SelectedDate = _defaultDate;
        }

        protected override void ClearValue()
        {
            base.ClearValue();
            SelectedDate = IsNullable ? (DateTime?)null : _defaultDate;
        }
    }
}