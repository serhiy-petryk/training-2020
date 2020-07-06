// Based on Mahapps

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace WpfInvestigate.Obsolete
{
    /// <summary>
    /// Interaction logic for NumericUpDown.xaml
    /// </summary>
    [TemplatePart(Name = ClearButtonPart, Type = typeof(Button))]
    [TemplatePart(Name = UpButtonPart, Type = typeof(RepeatButton))]
    [TemplatePart(Name = DownButtonPart, Type = typeof(RepeatButton))]
    [TemplatePart(Name = TextBoxPart, Type = typeof(TextBox))]
    public partial class NumericUpDown
    {
        static NumericUpDown() => DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));

        public NumericUpDown()
        {
            InitializeComponent();
        }

        private const string ClearButtonPart = "PART_ClearButton";
        private const string DownButtonPart = "PART_DownButton";
        private const string UpButtonPart = "PART_UpButton";
        private const string TextBoxPart = "PART_TextBox";
        private const decimal DefaultInterval = 1m;
        private const string ScientificNotationChar = "E";
        private const StringComparison StrComp = StringComparison.InvariantCultureIgnoreCase;

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<decimal?>), typeof(NumericUpDown));

        public static readonly DependencyProperty DelayProperty = DependencyProperty.Register("Delay", typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata(500), ValidateDelay);
        public static readonly DependencyProperty SpeedupProperty = DependencyProperty.Register("Speedup", typeof(bool), typeof(NumericUpDown), new FrameworkPropertyMetadata(true, OnSpeedupChanged));
        public static readonly DependencyProperty IsReadOnlyProperty = TextBoxBase.IsReadOnlyProperty.AddOwner(typeof(NumericUpDown), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, OnIsReadOnlyPropertyChanged));
        public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register("StringFormat", typeof(string), typeof(NumericUpDown), new FrameworkPropertyMetadata(string.Empty, OnStringFormatChanged, CoerceStringFormat));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(decimal?), typeof(NumericUpDown), new FrameworkPropertyMetadata(default(decimal?), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, CoerceValue));
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register("MinValue", typeof(decimal), typeof(NumericUpDown), new FrameworkPropertyMetadata(decimal.MinValue, OnMinValueChanged));
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(decimal), typeof(NumericUpDown), new FrameworkPropertyMetadata(decimal.MaxValue, OnMaxValueChanged, CoerceMaxValue));
        public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register("Interval", typeof(decimal), typeof(NumericUpDown), new FrameworkPropertyMetadata(DefaultInterval, OnIntervalChanged));
        public static readonly DependencyProperty InterceptMouseWheelProperty = DependencyProperty.Register("InterceptMouseWheel", typeof(bool), typeof(NumericUpDown), new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty TrackMouseWheelWhenMouseOverProperty = DependencyProperty.Register("TrackMouseWheelWhenMouseOver", typeof(bool), typeof(NumericUpDown), new FrameworkPropertyMetadata(default(bool)));
        public static readonly DependencyProperty ButtonsWidthProperty = DependencyProperty.Register("ButtonsWidth", typeof(double), typeof(NumericUpDown), new PropertyMetadata(16.0));
        public static readonly DependencyProperty InterceptManualEnterProperty = DependencyProperty.Register("InterceptManualEnter", typeof(bool), typeof(NumericUpDown), new PropertyMetadata(true, OnInterceptManualEnterChanged));
        public static readonly DependencyProperty CultureProperty = DependencyProperty.Register("Culture", typeof(CultureInfo), typeof(NumericUpDown), new PropertyMetadata(null, (o, e) => {
                if (e.NewValue != e.OldValue)
                {
                    var numUpDown = (NumericUpDown)o;
                    numUpDown.OnValueChanged(numUpDown.Value, numUpDown.Value);
                }
            }));

        public static readonly DependencyProperty SelectAllOnFocusProperty = DependencyProperty.Register("SelectAllOnFocus", typeof(bool), typeof(NumericUpDown), new PropertyMetadata(true));
        public static readonly DependencyProperty SnapToMultipleOfIntervalProperty = DependencyProperty.Register("SnapToMultipleOfInterval", typeof(bool), typeof(NumericUpDown), new PropertyMetadata(default(bool), OnSnapToMultipleOfIntervalChanged));
        
        private static void OnIsReadOnlyPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue && e.NewValue != null)
            {
                var numUpDown = (NumericUpDown)dependencyObject;
                var isReadOnly = (bool)e.NewValue;
                numUpDown.ToggleReadOnlyMode(isReadOnly || !numUpDown.InterceptManualEnter);
            }
        }

        private static void OnInterceptManualEnterChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue && e.NewValue != null)
            {
                var numUpDown = (NumericUpDown)dependencyObject;
                var interceptManualEnter = (bool)e.NewValue;
                numUpDown.ToggleReadOnlyMode(!interceptManualEnter || numUpDown.IsReadOnly);
            }
        }

        private static readonly Regex RegexStringFormatHexadecimal = new Regex(@"^(?<complexHEX>.*{\d:X\d+}.*)?(?<simpleHEX>X\d+)?$", RegexOptions.Compiled);
        private static readonly Regex RegexStringFormatNumber = new Regex(@"[-+]?(?<![0-9][.,])\b[0-9]+(?:[.,\s][0-9]+)*[.,]?[0-9]?(?:[eE][-+]?[0-9]+)?\b(?!\.[0-9])", RegexOptions.Compiled);

        private decimal _internalIntervalMultiplierForCalculation = DefaultInterval;
        private decimal _internalLargeChange = DefaultInterval * 100m;
        private decimal _intervalValueSinceReset;
        private bool _manualChange;
        private Button _clearButton;
        private RepeatButton _downButton;
        private RepeatButton _upButton;
        private TextBox _textBox;

        public event RoutedPropertyChangedEventHandler<decimal?> ValueChanged
        {
            add => AddHandler(ValueChangedEvent, value);
            remove => RemoveHandler(ValueChangedEvent, value);
        }

        /// <summary>
        ///     Gets or sets the amount of time, in milliseconds, the NumericUpDown waits while the up/down button is pressed
        ///     before it starts increasing/decreasing the
        ///     <see cref="Value" /> for the specified <see cref="Interval" /> . The value must be
        ///     non-negative.
        /// </summary>
        public int Delay
        {
            get => (int)GetValue(DelayProperty);
            set => SetValue(DelayProperty, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the user can use the mouse wheel to change values.
        /// </summary>
        public bool InterceptMouseWheel
        {
            get => (bool)GetValue(InterceptMouseWheelProperty);
            set => SetValue(InterceptMouseWheelProperty, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the control must have the focus in order to change values using the mouse wheel.
        /// <remarks>
        ///     If the value is true then the value changes when the mouse wheel is over the control. If the value is false then the value changes only if the control has the focus. If <see cref="InterceptMouseWheel"/> is set to "false" then this property has no effect.
        /// </remarks>
        /// </summary>
        public bool TrackMouseWheelWhenMouseOver
        {
            get => (bool)GetValue(TrackMouseWheelWhenMouseOverProperty);
            set => SetValue(TrackMouseWheelWhenMouseOverProperty, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the user can enter text in the control.
        /// </summary>
        public bool InterceptManualEnter
        {
            get => (bool)GetValue(InterceptManualEnterProperty);
            set => SetValue(InterceptManualEnterProperty, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating the culture to be used in string formatting operations.
        /// </summary>
        public CultureInfo Culture
        {
            get => (CultureInfo)GetValue(CultureProperty);
            set => SetValue(CultureProperty, value);
        }

        public double ButtonsWidth
        {
            get => (double)GetValue(ButtonsWidthProperty);
            set => SetValue(ButtonsWidthProperty, value);
        }

        public decimal Interval
        {
            get => (decimal)GetValue(IntervalProperty);
            set => SetValue(IntervalProperty, value);
        }

        public bool SelectAllOnFocus
        {
            get { return (bool)GetValue(SelectAllOnFocusProperty); }
            set { SetValue(SelectAllOnFocusProperty, value); }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the text can be changed by the use of the up or down buttons only.
        /// </summary>

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public decimal MaxValue
        {
            get => (decimal)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public decimal MinValue
        {
            get => (decimal)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the value to be added to or subtracted from <see cref="Value" /> remains
        ///     always
        ///     <see cref="Interval" /> or if it will increase faster after pressing the up/down button/arrow some time.
        /// </summary>
        public bool Speedup
        {
            get => (bool)GetValue(SpeedupProperty);
            set => SetValue(SpeedupProperty, value);
        }

        /// <summary>
        ///     Gets or sets the formatting for the displaying <see cref="Value" />
        /// </summary>
        /// <remarks>
        ///     <see href="http://msdn.microsoft.com/en-us/library/dwhawy9k.aspx"></see>
        /// </remarks>
        public string StringFormat
        {
            get => (string)GetValue(StringFormatProperty);
            set => SetValue(StringFormatProperty, value);
        }

        public decimal? Value
        {
            get => (decimal?)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private CultureInfo SpecificCultureInfo => Culture ?? Language.GetSpecificCulture();

        /// <summary>
        ///     Indicates if the NumericUpDown should round the value to the nearest possible interval when the focus moves to another element.
        /// </summary>
        public bool SnapToMultipleOfInterval
        {
            get => (bool)GetValue(SnapToMultipleOfIntervalProperty);
            set => SetValue(SnapToMultipleOfIntervalProperty, value);
        }

        /// <summary> 
        ///     Called when this element or any below gets focus.
        /// </summary>
        private void NumericUpDown_OnGotFocus(object sender, RoutedEventArgs e)
        {
            // When NumericUpDown gets logical focus, select the text inside us.
            // If we're an editable NumericUpDown, forward focus to the TextBox element
            if (!e.Handled)
            {
                var numericUpDown = (NumericUpDown)sender;
                if ((numericUpDown.InterceptManualEnter || numericUpDown.IsReadOnly) && numericUpDown.Focusable && e.OriginalSource == numericUpDown)
                {
                    // MoveFocus takes a TraversalRequest as its argument.
                    var request = new TraversalRequest((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift ? FocusNavigationDirection.Previous : FocusNavigationDirection.Next);
                    // Gets the element with keyboard focus.
                    var elementWithFocus = Keyboard.FocusedElement as UIElement;
                    // Change keyboard focus.
                    if (elementWithFocus != null)
                        elementWithFocus.MoveFocus(request);
                    else
                        numericUpDown.Focus();

                    e.Handled = true;
                }
            }
        }

        /// <summary>
        ///     When overridden in a derived class, is invoked whenever application code or internal processes call
        ///     <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _clearButton = GetTemplateChild(ClearButtonPart) as Button;

            _upButton = GetTemplateChild(UpButtonPart) as RepeatButton;
            _downButton = GetTemplateChild(DownButtonPart) as RepeatButton;

            _textBox = GetTemplateChild(TextBoxPart) as TextBox;
            if (_clearButton == null || _upButton == null || _downButton == null || _textBox == null )
                throw new InvalidOperationException(string.Format("You have missed to specify {0}, {1} or {2} in your template", UpButtonPart, DownButtonPart, TextBoxPart));

            ToggleReadOnlyMode(IsReadOnly || !InterceptManualEnter);

            _clearButton.Click += (o, e) => ClearButtonClicked();
            _upButton.Click += (o, e) => ChangeValueWithSpeedUp(true, false);
            _downButton.Click += (o, e) => ChangeValueWithSpeedUp(false, false);

            _upButton.PreviewMouseUp += (o, e) => ResetInternal();
            _downButton.PreviewMouseUp += (o, e) => ResetInternal();

            Value = (decimal?)CoerceValue(this, Value);
            OnValueChanged(Value, Value);
        }

        private void ToggleReadOnlyMode(bool isReadOnly)
        {
            if (_clearButton == null || _upButton == null || _downButton == null || _textBox == null)
                return;

            if (isReadOnly)
            {
                _textBox.GotFocus -= OnTextBoxGotFocus;
                _textBox.LostFocus -= OnTextBoxLostFocus;
                _textBox.PreviewTextInput -= OnPreviewTextInput;
                _textBox.PreviewKeyDown -= OnTextBoxKeyDown;
                _textBox.TextChanged -= OnTextChanged;
                DataObject.RemovePastingHandler(_textBox, OnValueTextBoxPaste);
            }
            else
            {
                _textBox.GotFocus += OnTextBoxGotFocus;
                _textBox.LostFocus += OnTextBoxLostFocus;
                _textBox.PreviewTextInput += OnPreviewTextInput;
                _textBox.PreviewKeyDown += OnTextBoxKeyDown;
                _textBox.TextChanged += OnTextChanged;
                DataObject.AddPastingHandler(_textBox, OnValueTextBoxPaste);
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            switch (e.Key)
            {
                case Key.Up:
                    ChangeValueWithSpeedUp(true, true);
                    e.Handled = true;
                    break;
                case Key.Down:
                    ChangeValueWithSpeedUp(false, true);
                    e.Handled = true;
                    break;
            }

            if (e.Handled)
            {
                _manualChange = false;
                InternalSetText(Value);
            }
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);

            if (e.Key == Key.Down || e.Key == Key.Up) 
                ResetInternal();
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            if (InterceptMouseWheel && (IsFocused || _textBox.IsFocused || TrackMouseWheelWhenMouseOver))
            {
                _manualChange = false;
                ChangeValueInternal(e.Delta > 0 ? Interval : -Interval);
            }
        }

        protected void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            IsInputError = false;
            e.Handled = true;
            if (e.Text == "\t") // tab key
                return;
            IsInputError = true;
            if (string.IsNullOrWhiteSpace(e.Text) || e.Text.Length != 1)
                return;

            var textBox = (TextBox)sender;
            var equivalentCulture = SpecificCultureInfo;
            var numberFormatInfo = equivalentCulture.NumberFormat;

            var text = e.Text;

            if (char.IsDigit(text[0]))
            {
                if (textBox.Text.IndexOf(numberFormatInfo.NegativeSign, textBox.SelectionStart + textBox.SelectionLength, StrComp) < 0
                    && textBox.Text.IndexOf(numberFormatInfo.PositiveSign, textBox.SelectionStart + textBox.SelectionLength, StrComp) < 0)
                {
                    e.Handled = false;
                }
            }
            else
            {
                var allTextSelected = textBox.SelectedText == textBox.Text;

                if (numberFormatInfo.NumberDecimalSeparator == text)
                {
                    if (textBox.Text.All(i => i.ToString(equivalentCulture) != numberFormatInfo.NumberDecimalSeparator) || allTextSelected)
                        if (HasValueDecimalPlaces)
                            e.Handled = false;
                }
                else
                {
                    if (numberFormatInfo.NegativeSign == text || text == numberFormatInfo.PositiveSign)
                    {
                        if (textBox.SelectionStart == 0)
                        {
                            // check if text already has a + or - sign
                            if (textBox.Text.Length > 1)
                            {
                                if (allTextSelected ||
                                    (!textBox.Text.StartsWith(numberFormatInfo.NegativeSign, StrComp) &&
                                    !textBox.Text.StartsWith(numberFormatInfo.PositiveSign, StrComp)))
                                {
                                    e.Handled = false;
                                }
                            }
                            else
                                e.Handled = false;
                        }
                        else if (textBox.SelectionStart > 0)
                        {
                            var elementBeforeCaret = textBox.Text.ElementAt(textBox.SelectionStart - 1).ToString(equivalentCulture);
                            if (elementBeforeCaret.Equals(ScientificNotationChar, StrComp) && HasValueDecimalPlaces)
                                e.Handled = false;
                        }
                    }
                    else if (text.Equals(ScientificNotationChar, StrComp) && HasValueDecimalPlaces && textBox.SelectionStart > 0 &&
                             !textBox.Text.Any(i => i.ToString(equivalentCulture).Equals(ScientificNotationChar, StrComp)))
                    {
                        e.Handled = false;
                    }
                }
            }

            IsInputError = e.Handled;
            _manualChange = _manualChange || !e.Handled;
        }

        protected virtual void OnSpeedupChanged(bool oldSpeedup, bool newSpeedup)
        {
        }

        /// <summary>
        ///     Raises the <see cref="ValueChanged" /> routed event.
        /// </summary>
        /// <param name="oldValue">
        ///     Old value of the <see cref="Value" /> property
        /// </param>
        /// <param name="newValue">
        ///     New value of the <see cref="Value" /> property
        /// </param>
        protected virtual void OnValueChanged(decimal? oldValue, decimal? newValue)
        {
            IsInputError = false;
            if (!_manualChange)
            {
                if (newValue.HasValue)
                {
                    if (_upButton != null && _upButton.IsEnabled != newValue < MaxValue)
                        _upButton.IsEnabled = newValue < MaxValue;
                    if (_downButton != null && _downButton.IsEnabled != newValue > MinValue)
                        _downButton.IsEnabled = newValue > MinValue;
                    if (newValue <= MinValue && newValue >= MinValue)
                        ResetInternal();
                    if (_textBox != null)
                        InternalSetText(newValue);
                }
                else
                {
                    if (_upButton != null && !_upButton.IsEnabled)
                        _upButton.IsEnabled = true;
                    if (_downButton != null && !_downButton.IsEnabled)
                        _downButton.IsEnabled = true;
                    if (_textBox != null)
                        _textBox.Text = null;
                }
            }

            if (oldValue != newValue)
                RaiseEvent(new RoutedPropertyChangedEventArgs<decimal?>(oldValue, newValue, ValueChangedEvent));
        }

        private static object CoerceMaxValue(DependencyObject d, object value)
        {
            var minValue = ((NumericUpDown)d).MinValue;
            var val = (decimal)value;
            return val < minValue ? minValue : val;
        }

        private static object CoerceStringFormat(DependencyObject d, object basevalue) => basevalue ?? string.Empty;

        private static object CoerceValue(DependencyObject d, object value)
        {
            var numericUpDown = (NumericUpDown)d;
            if (value == null && !numericUpDown.IsNullable)
                return Math.Max(0m, numericUpDown.MinValue);
            if (value == null)
                return null;

            var val = numericUpDown.TruncateValue(((decimal?)value).Value);
            if (val < numericUpDown.MinValue)
                return numericUpDown.MinValue;
            if (val > numericUpDown.MaxValue)
                return numericUpDown.MaxValue;
            return val;
        }

        private static void OnIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var numericUpDown = (NumericUpDown)d;
            numericUpDown.ResetInternal();
        }

        private static void OnMaxValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var numericUpDown = (NumericUpDown)d;
            numericUpDown.CoerceValue(ValueProperty);
            numericUpDown.EnableDisableUpDown();
        }

        private static void OnMinValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var numericUpDown = (NumericUpDown)d;
            numericUpDown.CoerceValue(MaxValueProperty);
            numericUpDown.CoerceValue(ValueProperty);
            numericUpDown.EnableDisableUpDown();
        }

        private static void OnSpeedupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (NumericUpDown)d;
            ctrl.OnSpeedupChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        private static void OnStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var numericUpDown = (NumericUpDown)d;
            if (numericUpDown._textBox != null && numericUpDown.Value.HasValue) 
                numericUpDown.InternalSetText(numericUpDown.Value);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var numericUpDown = (NumericUpDown)d;
            numericUpDown.OnValueChanged((decimal?)e.OldValue, (decimal?)e.NewValue);
        }

        private static void OnSnapToMultipleOfIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var numericUpDown = (NumericUpDown)d;
            var value = numericUpDown.Value.GetValueOrDefault();

            if ((bool)e.NewValue && Math.Abs(numericUpDown.Interval) > 0)
                numericUpDown.Value = Math.Round(value / numericUpDown.Interval) * numericUpDown.Interval;
        }

        private static void OnIsNullableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                var numUpDown = (NumericUpDown)d;
                numUpDown.Value = numUpDown.Value; // Coerce value
            }
        }

        private static bool ValidateDelay(object value) => Convert.ToInt32(value) >= 0;

        private void InternalSetText(decimal? newValue)
        {
            if (newValue.HasValue)
                _textBox.Text = FormattedValue(newValue, StringFormat, SpecificCultureInfo);
            else
                _textBox.Text = null;
        }

        private string FormattedValue(decimal? newValue, string format, CultureInfo culture)
        {
            format = format.Replace("{}", string.Empty);
            if (!string.IsNullOrWhiteSpace(format))
            {
                var match = RegexStringFormatHexadecimal.Match(format);
                if (match.Success)
                {
                    if (match.Groups["simpleHEX"].Success)
                    {
                        // HEX DOES SUPPORT INT ONLY.
                        return ((int)newValue.Value).ToString(match.Groups["simpleHEX"].Value, culture);
                    }
                    if (match.Groups["complexHEX"].Success)
                        return string.Format(culture, match.Groups["complexHEX"].Value, (int) newValue.Value);
                }
                else
                {
                    if (!format.Contains("{"))
                    {
                        // then we may have a StringFormat of e.g. "N0"
                        return newValue.Value.ToString(format, culture);
                    }
                    return string.Format(culture, format, newValue.Value);
                }
            }

            return newValue.Value.ToString(culture);
        }

        private void ChangeValueWithSpeedUp(bool toPositive, bool isManualChange)
        {
            if (IsReadOnly)
                return;

            _manualChange = isManualChange;
            decimal direction = toPositive ? 1 : -1;
            if (Speedup)
            {
                var d = Interval * _internalLargeChange;
                if ((_intervalValueSinceReset += Interval * _internalIntervalMultiplierForCalculation) > d)
                {
                    _internalLargeChange *= 10;
                    _internalIntervalMultiplierForCalculation *= 10;
                }
                ChangeValueInternal(direction * _internalIntervalMultiplierForCalculation);
            }
            else
                ChangeValueInternal(direction * Interval);
        }

        private void ChangeValueInternal(decimal interval)
        {
            if (IsReadOnly)
                return;

            Value = Value.GetValueOrDefault() + interval;
            _textBox.CaretIndex = _textBox.Text.Length;
        }

        private void EnableDisableUpDown()
        {
            if (_upButton != null) 
                _upButton.IsEnabled = Value < MaxValue;
            if (_downButton != null) 
                _downButton.IsEnabled = Value > MinValue;
        }

        private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            _manualChange = _manualChange || e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Decimal || e.Key == Key.OemComma || e.Key == Key.OemPeriod;

            if (HasValueDecimalPlaces && (e.Key == Key.Decimal || e.Key == Key.OemPeriod))
            {
                var textBox = sender as TextBox;

                if (textBox.Text.Contains(SpecificCultureInfo.NumberFormat.NumberDecimalSeparator) == false)
                {
                    //the control doesn't contai the decimal separator
                    //so we get the current caret index to insert the current culture decimal separator
                    var caret = textBox.CaretIndex;
                    //update the control text
                    textBox.Text = textBox.Text.Insert(caret, SpecificCultureInfo.NumberFormat.CurrencyDecimalSeparator);
                    //move the caret to the correct position
                    textBox.CaretIndex = caret + 1;
                }
                e.Handled = true;
            }
        }

        private void OnTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            if (SelectAllOnFocus)
                Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() => _textBox?.SelectAll()));
        }

        private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (!InterceptManualEnter)
                return;

            if (_manualChange)
            {
                var tb = (TextBox)sender;
                _manualChange = false;

                if (ValidateText(tb.Text, out var convertedValue))
                {
                    if (SnapToMultipleOfInterval && Math.Abs(Interval) > 0)
                        convertedValue = Math.Round(convertedValue / Interval) * Interval;

                    if (convertedValue > MaxValue)
                        convertedValue = MaxValue;
                    else if (convertedValue < MinValue)
                        convertedValue = MinValue;

                    SetCurrentValue(ValueProperty, convertedValue);
                }
            }

            OnValueChanged(Value, Value);
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(((TextBox) sender).Text))
                Value = null;
            else if (_manualChange)
                if (ValidateText(((TextBox) sender).Text, out var convertedValue))
                    SetCurrentValue(ValueProperty, convertedValue);
        }

        private void OnValueTextBoxPaste(object sender, DataObjectPastingEventArgs e)
        {
            var textBox = (TextBox)sender;
            var textPresent = textBox.Text;

            var isText = e.SourceDataObject.GetDataPresent(DataFormats.Text, true);
            if (!isText)
            {
                e.CancelCommand();
                return;
            }

            var text = e.SourceDataObject.GetData(DataFormats.Text) as string;

            var newText = string.Concat(textPresent.Substring(0, textBox.SelectionStart), text, textPresent.Substring(textBox.SelectionStart + textBox.SelectionLength));
            if (!ValidateText(newText, out _))
                e.CancelCommand();
            else
                _manualChange = true;
        }

        private void ResetInternal()
        {
            if (IsReadOnly)
                return;

            _internalLargeChange = 100 * Interval;
            _internalIntervalMultiplierForCalculation = Interval;
            _intervalValueSinceReset = 0;
        }

        private bool ValidateText(string text, out decimal convertedValue)
        {
            text = GetAnyNumberFromText(text);
            return decimal.TryParse(text, NumberStyles.Any, SpecificCultureInfo, out convertedValue);
        }

        private string GetAnyNumberFromText(string text)
        {
            var matches = RegexStringFormatNumber.Matches(text);
            return matches.Count > 0 ? matches[0].Value : text;
        }

        private void ClearButtonClicked()
        {
            _manualChange = false;
            Value = null;
        }

        //============================================
        public static readonly DependencyProperty IsInputErrorProperty = DependencyProperty.Register("IsInputError", typeof(bool), typeof(NumericUpDown), new FrameworkPropertyMetadata(false));
        /// <summary>
        /// Can the value be null?
        /// </summary>
        public bool IsInputError
        {
            get => (bool)GetValue(IsInputErrorProperty);
            set => SetValue(IsInputErrorProperty, value);
        }

        public static readonly DependencyProperty IsNullableProperty = DependencyProperty.Register("IsNullable", typeof(bool), typeof(NumericUpDown), new FrameworkPropertyMetadata(false, OnIsNullableChanged));
        /// <summary>
        /// Can the value be null?
        /// </summary>
        public bool IsNullable
        {
            get => (bool)GetValue(IsNullableProperty);
            set => SetValue(IsNullableProperty, value);
        }

        public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register("DecimalPlaces", typeof(int?), typeof(NumericUpDown), new FrameworkPropertyMetadata(null, OnDecimalPlacesChanged, CoerceDecimalPlaces));
        /// <summary>
        /// Rounding decimal places (from 0 to +19); null - not rounding
        /// </summary>
        public int? DecimalPlaces
        {
            get => (int?)GetValue(DecimalPlacesProperty);
            set => SetValue(DecimalPlacesProperty, value);
        }
        private static void OnDecimalPlacesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var numericUpDown = (NumericUpDown)d;
            if (e.NewValue != e.OldValue && numericUpDown.Value.HasValue)
                numericUpDown.Value = numericUpDown.TruncateValue(numericUpDown.Value.Value);
        }
        private static object CoerceDecimalPlaces(DependencyObject d, object value)
        {
            var val = (int?)value;
            if (val != null && val.Value < 0)
                return 0;
            if (val != null && val.Value > 19)
                return 19;
            return val;
        }

        private decimal TruncateValue(decimal value)
        {
            if (!DecimalPlaces.HasValue)
                return value;

            var factor = Convert.ToDecimal(Math.Pow(10, DecimalPlaces.Value));
            return Math.Truncate(value * factor) / factor;
        }

        private bool HasValueDecimalPlaces => !(DecimalPlaces.HasValue && DecimalPlaces.Value <= 0);

    }
}
