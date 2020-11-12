using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    public partial class NumericBox: INotifyPropertyChanged
    {
        [Flags]
        public enum Buttons
        {
            None = 0,
            Close = 1,
            Calculator = 2,
            LeftDown = 4,
            RightDown = 8,
            Up = 16
        }

        private int _numberOfIntervals = 0;

        static NumericBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericBox), new FrameworkPropertyMetadata(typeof(NumericBox)));
        public NumericBox()
        {
            InitializeComponent();
            Culture = Tips.CurrentCulture;
        }

        private const decimal DefaultInterval = 1m;
        private const string ScientificNotationChar = "E";
        private const StringComparison StrComp = StringComparison.InvariantCultureIgnoreCase;

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<decimal?>), typeof(NumericBox));

        public static readonly DependencyProperty IsReadOnlyProperty = TextBoxBase.IsReadOnlyProperty.AddOwner(typeof(NumericBox), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, OnIsReadOnlyPropertyChanged));
        public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register("StringFormat", typeof(string), typeof(NumericBox), new FrameworkPropertyMetadata(string.Empty, OnStringFormatChanged, CoerceStringFormat));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(decimal?), typeof(NumericBox), new FrameworkPropertyMetadata(default(decimal?), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, CoerceValue));
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register("MinValue", typeof(decimal), typeof(NumericBox), new FrameworkPropertyMetadata(decimal.MinValue, OnMinValueChanged));
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(decimal), typeof(NumericBox), new FrameworkPropertyMetadata(decimal.MaxValue, OnMaxValueChanged, CoerceMaxValue));
        public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register("Interval", typeof(decimal), typeof(NumericBox), new FrameworkPropertyMetadata(DefaultInterval, OnIntervalChanged));
        public static readonly DependencyProperty VisibleButtonsProperty = DependencyProperty.Register("VisibleButtons", typeof(Buttons), typeof(NumericBox), new FrameworkPropertyMetadata(Buttons.Close|Buttons.Calculator, OnVisibleButtonsChanged));

        public readonly CultureInfo Culture;
        public bool IsCloseButtonVisible => (VisibleButtons & Buttons.Close) == Buttons.Close;
        public bool IsCalculatorButtonVisible => (VisibleButtons & Buttons.Calculator) == Buttons.Calculator;
        public bool IsDownButtonsVisible => (VisibleButtons & Buttons.LeftDown) == Buttons.LeftDown || (VisibleButtons & Buttons.RightDown) == Buttons.RightDown;
        public bool IsUpButtonsVisible => (VisibleButtons & Buttons.Up) == Buttons.Up;
        public bool IsTextBoxLeftBorderVisible => (VisibleButtons & Buttons.LeftDown) == Buttons.LeftDown;
        public bool IsTextBoxRightBorderVisible => (VisibleButtons & Buttons.Calculator) == Buttons.Calculator || (VisibleButtons & Buttons.RightDown) == Buttons.RightDown || (VisibleButtons & Buttons.Up) == Buttons.Up || (VisibleButtons & Buttons.Close) == Buttons.Close;
        public int DownButtonColumn => (VisibleButtons & Buttons.LeftDown) == Buttons.LeftDown ? 0 : 5;

        private static readonly Regex RegexStringFormatHexadecimal = new Regex(@"^(?<complexHEX>.*{\d:X\d+}.*)?(?<simpleHEX>X\d+)?$", RegexOptions.Compiled);
        private static readonly Regex RegexStringFormatNumber = new Regex(@"[-+]?(?<![0-9][.,])\b[0-9]+(?:[.,\s][0-9]+)*[.,]?[0-9]?(?:[eE][-+]?[0-9]+)?\b(?!\.[0-9])", RegexOptions.Compiled);
        private string _decimalSeparator => Culture.NumberFormat.NumberDecimalSeparator;

        private bool _manualChange;
        private Button _clearButton;
        private RepeatButton _downButton;
        private RepeatButton _upButton;
        private TextBox _textBox;
        private Popup _popup;
        private ToggleButton _toggleButton;

        public event RoutedPropertyChangedEventHandler<decimal?> ValueChanged
        {
            add => AddHandler(ValueChangedEvent, value);
            remove => RemoveHandler(ValueChangedEvent, value);
        }

        public decimal Interval
        {
            get => (decimal)GetValue(IntervalProperty);
            set => SetValue(IntervalProperty, value);
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

        public Buttons VisibleButtons
        {
            get => (Buttons)GetValue(VisibleButtonsProperty);
            set => SetValue(VisibleButtonsProperty, value);
        }

        /// <summary> 
        ///     Called when this element or any below gets focus.
        /// </summary>
        private void OnNumericBoxGotFocus(object sender, RoutedEventArgs e)
        {
            // When NumericBox gets logical focus, select the text inside us.
            // If we're an editable NumericBox, forward focus to the TextBox element
            if (!e.Handled)
            {
                var numericBox = (NumericBox)sender;
                if (!numericBox.IsReadOnly && numericBox.Focusable && e.OriginalSource == numericBox)
                {
                    // MoveFocus takes a TraversalRequest as its argument.
                    var request = new TraversalRequest((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift ? FocusNavigationDirection.Previous : FocusNavigationDirection.Next);
                    // Gets the element with keyboard focus.
                    var elementWithFocus = Keyboard.FocusedElement as UIElement;
                    // Change keyboard focus.
                    if (elementWithFocus != null)
                        elementWithFocus.MoveFocus(request);
                    else
                        numericBox.Focus();

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

            _clearButton = GetTemplateChild("PART_ClearButton") as Button;
            _upButton = GetTemplateChild("PART_UpButton") as RepeatButton;
            _downButton = GetTemplateChild("PART_DownButton") as RepeatButton;
            _textBox = GetTemplateChild("PART_TextBox") as TextBox;
            _popup = GetTemplateChild("PART_Popup") as Popup;
            _toggleButton=GetTemplateChild("PART_ToggleButton") as ToggleButton;

            if (_clearButton == null || _upButton == null || _downButton == null || _textBox == null || _popup == null || _toggleButton == null)
                throw new InvalidOperationException("Invalid control template");

            ToggleReadOnlyMode();

            _clearButton.Click += (o, e) => ClearButtonClicked();
            _upButton.Click += (o, e) => ChangeValueWithSpeedUp(true, false);
            _downButton.Click += (o, e) => ChangeValueWithSpeedUp(false, false);
            _popup.Opened += (o, e) => _popup.Child.Focus();

            Value = (decimal?)CoerceValue(this, Value);
            OnValueChanged(Value, Value);
        }

        private void ToggleReadOnlyMode()
        {
            if (_clearButton == null || _upButton == null || _downButton == null || _textBox == null)
                return;

            if (IsReadOnly)
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

            if (_popup.IsOpen)
                return;

            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                ChangeValueWithSpeedUp(e.Key == Key.Up, true);
                _manualChange = false;
                InternalSetText(Value);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                _textBox.SelectAll();
                e.Handled = true;
            }
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);
            ResetInternal();
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            ResetInternal();
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            if (IsFocused || _textBox.IsFocused)
            {
                _manualChange = false;
                ChangeValueInternal(e.Delta > 0 ? Interval : -Interval);
            }
        }

        protected void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
            if (e.Text == "\t") // tab key
                return;
            if (string.IsNullOrWhiteSpace(e.Text) || e.Text.Length != 1)
            {
                Tips.Beep();
                return;
            }

            var textBox = (TextBox)sender;
            var numberFormat = Culture.NumberFormat;

            var text = e.Text;

            if (char.IsDigit(text[0]))
            {
                if (textBox.Text.IndexOf(numberFormat.NegativeSign, textBox.SelectionStart + textBox.SelectionLength, StrComp) < 0
                    && textBox.Text.IndexOf(numberFormat.PositiveSign, textBox.SelectionStart + textBox.SelectionLength, StrComp) < 0)
                {
                    e.Handled = false;
                }
            }
            else
            {
                var allTextSelected = textBox.SelectedText == textBox.Text;

                if (numberFormat.NumberDecimalSeparator == text)
                {
                    if (textBox.Text.All(i => i.ToString(Culture) != numberFormat.NumberDecimalSeparator) || allTextSelected)
                        if (HasDecimalPlaces)
                            e.Handled = false;
                }
                else
                {
                    if (numberFormat.NegativeSign == text || text == numberFormat.PositiveSign)
                    {
                        if (textBox.SelectionStart == 0)
                        {
                            // check if text already has a + or - sign
                            if (textBox.Text.Length > 1)
                            {
                                if (allTextSelected ||
                                    (!textBox.Text.StartsWith(numberFormat.NegativeSign, StrComp) &&
                                    !textBox.Text.StartsWith(numberFormat.PositiveSign, StrComp)))
                                {
                                    e.Handled = false;
                                }
                            }
                            else
                                e.Handled = false;
                        }
                        else if (textBox.SelectionStart > 0)
                        {
                            var elementBeforeCaret = textBox.Text.ElementAt(textBox.SelectionStart - 1).ToString(Culture);
                            if (elementBeforeCaret.Equals(ScientificNotationChar, StrComp) && HasDecimalPlaces)
                                e.Handled = false;
                        }
                    }
                    else if (text.Equals(ScientificNotationChar, StrComp) && HasDecimalPlaces && textBox.SelectionStart > 0 &&
                             !textBox.Text.Any(i => i.ToString(Culture).Equals(ScientificNotationChar, StrComp)))
                    {
                        e.Handled = false;
                    }
                }
            }

            if (e.Handled)
                Tips.Beep();
            _manualChange = _manualChange || !e.Handled;
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
            var minValue = ((NumericBox)d).MinValue;
            var val = (decimal)value;
            return val < minValue ? minValue : val;
        }

        private static object CoerceStringFormat(DependencyObject d, object basevalue) => basevalue ?? string.Empty;

        private static object CoerceValue(DependencyObject d, object value)
        {
            var numericBox = (NumericBox)d;

            if (numericBox.IsLoaded && numericBox.IsReadOnly)
                return numericBox.Value;

            if (value == null && !numericBox.IsNullable)
                return Math.Max(0m, numericBox.MinValue);
            if (value == null)
                return null;

            var val = numericBox.TruncateValue(((decimal?)value).Value);
            if (val < numericBox.MinValue)
                return numericBox.MinValue;
            if (val > numericBox.MaxValue)
                return numericBox.MaxValue;
            return val;
        }

        private static void OnIsReadOnlyPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((NumericBox)dependencyObject).ToggleReadOnlyMode();
        }

        private static void OnIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NumericBox)d).ResetInternal();
        }

        private static void OnMaxValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var numericBox = (NumericBox)d;
            numericBox.CoerceValue(ValueProperty);
            numericBox.EnableDisableUpDown();
        }

        private static void OnMinValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var numericBox = (NumericBox)d;
            numericBox.CoerceValue(MaxValueProperty);
            numericBox.CoerceValue(ValueProperty);
            numericBox.EnableDisableUpDown();
        }

        private static void OnStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var numericBox = (NumericBox)d;
            if (numericBox._textBox != null && numericBox.Value.HasValue)
                numericBox.InternalSetText(numericBox.Value);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NumericBox)d).OnValueChanged((decimal?)e.OldValue, (decimal?)e.NewValue);
        }

        private static void OnIsNullableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                var numericBox = (NumericBox)d;
                numericBox.Value = numericBox.Value; // Coerce value
            }
        }

        private static void OnVisibleButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NumericBox)d).OnPropertiesChanged(new[] { nameof(IsCloseButtonVisible), nameof(IsCalculatorButtonVisible), nameof(IsUpButtonsVisible), nameof(IsDownButtonsVisible), nameof(DownButtonColumn), nameof(IsTextBoxLeftBorderVisible), nameof(IsTextBoxRightBorderVisible) });
        }

        // ================================================
        private void InternalSetText(decimal? newValue)
        {
            if (newValue.HasValue)
                _textBox.Text = FormattedValue(newValue, StringFormat, Culture);
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
                        return string.Format(culture, match.Groups["complexHEX"].Value, (int)newValue.Value);
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
            var speedUpMultiplier = Calculator.GetIntervalMultiplier(_numberOfIntervals++);
            ChangeValueInternal((toPositive ? 1 : -1) * speedUpMultiplier * Interval);
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

            if (HasDecimalPlaces && (e.Key == Key.Decimal || (_decimalSeparator == "." && e.Key == Key.OemPeriod) || (_decimalSeparator == "," && e.Key == Key.OemComma)))
            {
                var textBox = sender as TextBox;

                if (textBox.Text.Contains(Culture.NumberFormat.NumberDecimalSeparator) == false)
                {
                    //the control doesn't contai the decimal separator
                    //so we get the current caret index to insert the current culture decimal separator
                    var caret = textBox.CaretIndex;
                    //update the control text
                    textBox.Text = textBox.Text.Insert(caret, Culture.NumberFormat.CurrencyDecimalSeparator);
                    //move the caret to the correct position
                    textBox.CaretIndex = caret + 1;
                }
                e.Handled = true;
            }
        }

        private void OnTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() => _textBox?.SelectAll()));
        }

        private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (_manualChange)
            {
                var tb = (TextBox)sender;
                _manualChange = false;

                if (ValidateText(tb.Text, out var convertedValue))
                {
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
            if (string.IsNullOrEmpty(((TextBox)sender).Text))
                Value = null;
            else if (_manualChange)
                if (ValidateText(((TextBox)sender).Text, out var convertedValue))
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
            _numberOfIntervals = 0;
        }

        private bool ValidateText(string text, out decimal convertedValue)
        {
            text = GetAnyNumberFromText(text);
            return decimal.TryParse(text, NumberStyles.Any, Culture, out convertedValue);
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
        public static readonly DependencyProperty IsNullableProperty = DependencyProperty.Register("IsNullable", typeof(bool), typeof(NumericBox), new FrameworkPropertyMetadata(false, OnIsNullableChanged));
        /// <summary>
        /// Can the value be null?
        /// </summary>
        public bool IsNullable
        {
            get => (bool)GetValue(IsNullableProperty);
            set => SetValue(IsNullableProperty, value);
        }

        public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register("DecimalPlaces", typeof(int?), typeof(NumericBox), new FrameworkPropertyMetadata(null, OnDecimalPlacesChanged, CoerceDecimalPlaces));
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
            var numericBox = (NumericBox)d;
            if (e.NewValue != e.OldValue && numericBox.Value.HasValue)
                numericBox.Value = numericBox.TruncateValue(numericBox.Value.Value);
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

        private bool HasDecimalPlaces => !(DecimalPlaces.HasValue && DecimalPlaces.Value <= 0);

        #region ===========  INotifyPropertyChanged  ===============
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertiesChanged(string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
