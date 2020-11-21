using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    public class Calculator : UserControl, INotifyPropertyChanged
    {
        private const int IntervalCountWhenChange = 100;
        private const int IntervalStepChange = 10;
        private int _numberOfIntervals = 0;

        internal static decimal GetIntervalMultiplier(int numberOfIntervals)
        {
            var power = numberOfIntervals / IntervalCountWhenChange;
            var result = 1m;
            for (var k = 0; k < power; k++)
                result *= IntervalStepChange;
            return result;

        }

        public Calculator()
        {
            DataContext = this;
            ClickCommand = new RelayCommand(ButtonClickHandler);
            PreviewMouseLeftButtonDown += Calculator_OnPreviewMouseLeftButtonDown;
            Culture = Tips.CurrentCulture;
        }

        public readonly CultureInfo Culture;
        public RelayCommand ClickCommand { get; }
        public string IndicatorText { get; private set; } = "0";
        public string ErrorText { get; private set; }

        private decimal? _firstOperand;
        private string _operator;
        private bool _lastButtonIsDigit = true;
        private decimal SecondOperand => decimal.Parse(IndicatorText, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, Culture);

        public string StatusText => _firstOperand.HasValue && !string.IsNullOrEmpty(_operator)
            ? _firstOperand.Value.ToString(Culture) + " " + _operator
            : "";

        public string DecimalSeparator => Culture.NumberFormat.NumberDecimalSeparator;

        #region ========  Override && events =================
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var indicator = GetTemplateChild("PART_Indicator") as TextBox;
            if (indicator != null)
            {
                indicator.PreviewKeyDown += Indicator_OnPreviewKeyDown;
                indicator.PreviewKeyUp += Indicator_OnPreviewKeyUp;
                indicator.PreviewMouseWheel += Indicator_OnPreviewMouseWheel;
                indicator.TextChanged += Indicator_OnTextChanged;
            }
        }
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            ActivateIndicator();
        }

        private void Calculator_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ActivateIndicator();
            ResetInterval();
        }

        private static Dictionary<Key, string> _keys = new Dictionary<Key, string>
        {
            {Key.D0, "0"}, {Key.D1, "1"}, {Key.D2, "2"}, {Key.D3, "3"}, {Key.D4, "4"}, {Key.D5, "5"}, {Key.D6, "6"},
            {Key.D7, "7"}, {Key.D8, "8"}, {Key.D9, "9"}, {Key.NumPad0, "0"}, {Key.NumPad1, "1"}, {Key.NumPad2, "2"},
            {Key.NumPad3, "3"}, {Key.NumPad4, "4"}, {Key.NumPad5, "5"}, {Key.NumPad6, "6"}, {Key.NumPad7, "7"},
            {Key.NumPad8, "8"}, {Key.NumPad9, "9"}, {Key.Up, "++"}, {Key.Down, "--"}, {Key.C, "Clear"},
            {Key.Back, "Backspace"}, {Key.Left, "Backspace"}, {Key.Divide, "/"}, {Key.Multiply, "*"},
            {Key.Subtract, "-"}, {Key.OemMinus, "-"}, {Key.Add, "+"}, {Key.Return, "="}, {Key.OemPlus, "="},
            {Key.Decimal, "."}, {Key.OemPeriod, "."}, {Key.OemComma, "."}, {Key.OemQuestion, "/"},
            {Key.Delete, "Delete"}
        };
        private static Dictionary<Key, string> _shiftKeys = new Dictionary<Key, string>
        {
            {Key.D8, "*"}, {Key.OemPlus, "+"}
        };
        private void Indicator_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (_keys.ContainsKey(e.Key))
                {
                    ButtonClickHandler(_keys[e.Key]);
                    e.Handled = true;
                }
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift && _shiftKeys.ContainsKey(e.Key))
            {
                var content = _shiftKeys[e.Key];
                var button = Tips.GetVisualChildren(this).OfType<ButtonBase>().FirstOrDefault(b => Equals(b.Tag, content));
                button?.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                e.Handled = true;
            }

            if (!e.Handled && e.Key.ToString().Length == 1)
                Tips.Beep();
        }
        private void Indicator_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            ResetInterval();
        }

        private void Indicator_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            // Adjust font size
            var maxFontSize = textBox.Name == "PART_Indicator" ? 20.0 : 14.0;
            if (textBox.Text.Length > 5)
            {
                textBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                textBox.FontSize = Math.Min(maxFontSize, textBox.FontSize * textBox.ActualWidth / textBox.DesiredSize.Width);
            }
            else
                textBox.FontSize = maxFontSize;
        }
        #endregion

        // ============================
        private void ActivateIndicator()
        {
            var indicator = GetTemplateChild("PART_Indicator") as FrameworkElement;
            indicator?.Focus();
        }

        private void ButtonClickHandler(object p)
        {
            if (char.IsDigit(((string)p)[0]))
                ExecuteDigit((string)p);
            else
                ExecuteOperator((string)p);
        }

        private void ExecuteDigit(string digit)
        {
            if (!string.IsNullOrEmpty(ErrorText))
                Clear();

            CheckLastButtonIsDigit(true);

            if (IndicatorText == "0")
                IndicatorText = digit;
            else
                IndicatorText += digit;
            _lastButtonIsDigit = true;
            RefrestUI();
        }

        private void ExecuteOperator(string cmd)
        {
            if (!string.IsNullOrEmpty(ErrorText) && cmd != "Clear")
                return;

            switch (cmd)
            {
                case "Delete":
                    CheckLastButtonIsDigit(false);
                    IndicatorText = "0";
                    break;

                case "++":
                    CheckLastButtonIsDigit(false);
                    var multiplier = GetIntervalMultiplier(_numberOfIntervals++);
                    IndicatorText = (SecondOperand + multiplier).ToString(Culture);
                    break;

                case "--":
                    CheckLastButtonIsDigit(false);
                    var multiplier1 = GetIntervalMultiplier(_numberOfIntervals++);
                    IndicatorText = (SecondOperand - multiplier1).ToString(Culture);
                    break;

                case ".":
                    CheckLastButtonIsDigit(true);
                    if (IndicatorText.Contains(DecimalSeparator))
                        IndicatorText = IndicatorText.Replace(DecimalSeparator, "");
                    IndicatorText += DecimalSeparator;
                    break;

                case "Backspace":
                    CheckLastButtonIsDigit(false);
                    IndicatorText = IndicatorText.Substring(0, IndicatorText.Length - 1);
                    if (IndicatorText.Length == 0 || IndicatorText == "-")
                        IndicatorText = "0";
                    break;

                case "Sign":
                    CheckLastButtonIsDigit(false);
                    if (!string.Equals(IndicatorText, "0", StringComparison.Ordinal))
                    {
                        if (IndicatorText.StartsWith("-"))
                            IndicatorText = IndicatorText.Substring(1);
                        else
                            IndicatorText = "-" + IndicatorText;
                    }
                    break;

                case "Clear":
                    Clear();
                    break;

                case "=":
                    if (string.IsNullOrEmpty(_operator) || !_firstOperand.HasValue)
                    {
                        Value = SecondOperand;
                        _lastButtonIsDigit = true;
                    }
                    else
                    {
                        Calculate();
                        if (string.IsNullOrEmpty(ErrorText))
                        {
                            Value = _firstOperand;
                            _firstOperand = null;
                            _lastButtonIsDigit = false;
                        }
                    }
                    break;

                default:
                    if (_firstOperand.HasValue && !string.IsNullOrEmpty(_operator) && _lastButtonIsDigit)
                    { // do operation
                        Calculate();
                        if (string.IsNullOrEmpty(ErrorText))
                        {
                            _operator = cmd;
                            IndicatorText = "0";
                        }
                    }
                    else if (_firstOperand.HasValue && !string.IsNullOrEmpty(_operator) && !_lastButtonIsDigit)
                        // change operand
                        _operator = cmd;
                    else if (!_firstOperand.HasValue && string.IsNullOrEmpty(_operator))
                    { // add operand
                        _firstOperand = SecondOperand;
                        _operator = cmd;
                        _lastButtonIsDigit = false;
                        IndicatorText = "0";
                    }
                    else
                        throw new Exception("Unknown state!!!");

                    break;
            }

            RefrestUI();
        }

        private void Calculate()
        {
            try
            {
                switch (_operator)
                {
                    case "+":
                        _firstOperand += SecondOperand;
                        break;
                    case "-":
                        _firstOperand -= SecondOperand;
                        break;
                    case "*":
                        _firstOperand *= SecondOperand;
                        break;
                    case "/":
                        if (SecondOperand == 0M)
                            ErrorText = "?? /0";
                        else
                            _firstOperand /= SecondOperand;
                        break;
                }

                _operator = null;
            }
            catch (OverflowException exception)
            {
                ErrorText = "?? Overflow";
            }
            catch (Exception exception)
            {
                ErrorText = "??";
            }

            if (!string.IsNullOrEmpty(ErrorText)) { 
                _firstOperand = null;
                _operator = null;
            }

            _lastButtonIsDigit = false;
        }

        private void CheckLastButtonIsDigit(bool resetValue)
        {
            if (!_lastButtonIsDigit)
            {
                if (resetValue)
                    IndicatorText = "0";
                _lastButtonIsDigit = true;
            }
        }

        private void OnValueChanged(decimal? newValue)
        {
            Clear();
            IndicatorText = newValue.HasValue ? newValue.Value.ToString(Culture) : "0";
            RefrestUI();
        }

        private void Clear()
        {
            _firstOperand = null;
            _lastButtonIsDigit = true;
            _operator = null;
            IndicatorText = "0";
            ErrorText = null;
        }

        private void ResetInterval()
        {
            _numberOfIntervals = 0;
        }

        private void RefrestUI()
        {
            OnPropertiesChanged(new[] { nameof(IndicatorText), nameof(StatusText), nameof(ErrorText), nameof(DecimalSeparator) });
        }

        #region ===========  INotifyPropertyChanged  ===============
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertiesChanged(string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private void Indicator_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var indicator = GetTemplateChild("PART_Indicator") as FrameworkElement;
            if (indicator.IsFocused)
                ExecuteOperator(e.Delta > 0 ? "++" : "--");
        }

        #region ===========  DependencyProperty  =================
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(decimal?),
            typeof(Calculator), new PropertyMetadata(null, (d, e) => ((Calculator)d).OnValueChanged(e.NewValue as decimal?)));

        public decimal? Value
        {
            get => (decimal?)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        #endregion
    }
}
