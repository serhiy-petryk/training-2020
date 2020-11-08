// ToDo:
// 1. ColorBox for Color/CurrentColor.
// 2. ToggleButton + popup
//      may change layout to: ToggleButton -> Grid -> (ContentPresenter + Popup)
//      зовнішній вигляд toggleButton = зовнішній вигляд Content
// 3. Control with 4 tab:
//      - mini size: RGB sliders
//      - mini size: HSL sliders
//      - middle size: current view
//      - middle size with scroll: color boxes of known colors (140 items)

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    public partial class ColorControl : UserControl, INotifyPropertyChanged
    {
        private ColorControlViewModel VM => (ColorControlViewModel)DataContext;

        // Constructor
        public ColorControl()
        {
            InitializeComponent();
            SaveColor();
        }

        #region  ==============  User UI  ===========
        /// <summary>Original color</summary>
        public Color Color
        {
            get => VM.Color;
            set
            {
                VM.Color = value;
                OnPropertiesChanged(nameof(Color), nameof(ColorBrush));
            }
        }
        public SolidColorBrush ColorBrush => new SolidColorBrush(Color);

        public void SaveColor()
        {
            VM.SaveColor();
            OnPropertiesChanged(nameof(Color), nameof(ColorBrush));
        }

        public void RestoreColor()
        {
            VM.RestoreColor();
            OnPropertiesChanged(nameof(Color), nameof(ColorBrush));
        }
        #endregion

        #region ==============  Event handlers  ====================

        private void Control_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            foreach (var cc in Tips.GetVisualChildren(fe).OfType<Canvas>().Where(cc => cc.DataContext is ColorControlViewModel.XYSlider))
                ((ColorControlViewModel.XYSlider)cc.DataContext).SetSizeOfControl(cc);

            VM.UpdateUI();
        }
        #endregion

        #region ================  SelectAll Event Handlers on Focus event  ===============
        private void ValueEditor_OnGotFocus(object sender, RoutedEventArgs e) => ((TextBox)sender).SelectAll();

        private void ValueEditor_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // select all text on got focus: see BillBR comment in https://stackoverflow.com/questions/660554/how-to-automatically-select-all-text-on-focus-in-wpf-textbox
            var textBox = (TextBox)sender;
            if (!textBox.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                textBox.Focus();
            }
        }
        #endregion

        #region ================  Event Handlers for Value Editor  ===============
        private void ValueEditor_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
            if (e.Text == "\t") // tab key
                return;
            if (string.IsNullOrWhiteSpace(e.Text) || e.Text.Length != 1)
            {
                Tips.Beep();
                return;
            }

            var valueEditor = (TextBox)sender;
            var newText = valueEditor.Text.Substring(0, valueEditor.SelectionStart) + e.Text +
                          valueEditor.Text.Substring(valueEditor.SelectionStart + valueEditor.SelectionLength);
            if (VM.CurrentCulture.NumberFormat.NativeDigits.Contains(e.Text))
                e.Handled = false;
            if (VM.CurrentCulture.NumberFormat.NumberDecimalSeparator == e.Text)
                e.Handled = valueEditor.Text.Contains(VM.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            else if (VM.CurrentCulture.NumberFormat.NegativeSign == e.Text)
                e.Handled = (newText.Length > 1 && newText.Substring(1, newText.Length - 2)
                                 .Contains(VM.CurrentCulture.NumberFormat.NegativeSign)) ||
                            !(newText.StartsWith(e.Text) || newText.EndsWith(e.Text));

            if (e.Handled)
                Tips.Beep();
        }
        #endregion

        #region ===============  Color box event handlers  ===============
        private void ColorBox_OnSetColor(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            var toggleButton = Tips.GetVisualParents(element).OfType<Grid>().SelectMany(grid => grid.Children.OfType<ToggleButton>()).FirstOrDefault();
            toggleButton.IsChecked = false;

            (element.DataContext as ColorControlViewModel.ColorToneBox).SetCurrentColor();
        }
        #endregion

        #region  =============  Slider event handlers  =====================
        private void Slider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (sender as UIElement).CaptureMouse();
            Keyboard.ClearFocus();
        }

        private void Slider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            (sender as UIElement).ReleaseMouseCapture();
        }

        private void Slider_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var canvas = sender as Panel;
                var thumb = canvas.Children[0] as FrameworkElement;
                var isVertical = thumb is Grid;
                var x = Math.Max(0, Math.Min(1.0, isVertical
                    ? e.GetPosition(canvas).X / canvas.ActualWidth
                    : (e.GetPosition(canvas).X - thumb.ActualWidth / 2) / (canvas.ActualWidth - thumb.ActualWidth)));
                var y = Math.Max(0, Math.Min(1.0, e.GetPosition(canvas).Y / canvas.ActualHeight));
                ((ColorControlViewModel.XYSlider)canvas.DataContext).SetProperties(x, y);
            }
        }
        #endregion

        #region ===========  INotifyPropertyChanged  ===============
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
