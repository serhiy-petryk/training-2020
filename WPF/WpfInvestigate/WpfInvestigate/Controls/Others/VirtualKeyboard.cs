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
using WpfInvestigate.Common.ColorSpaces;
using WpfInvestigate.Helpers;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Interaction logic for VirtualKeyboard.xaml
    /// </summary>
    public partial class VirtualKeyboard : UserControl, INotifyPropertyChanged
    {
        private const string DefaultLanguage = "EN";

        public VirtualKeyboard()
        {
            DataContext = this;

            AvailableKeyboardLayouts = CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures)
                .Where(a => KeyModel.KeyDefinition.LanguageDefinitions.Keys.Contains(a.IetfLanguageTag.ToUpper())).OrderBy(a => a.DisplayName)
                .Select(a => new LanguageModel(a.IetfLanguageTag)).ToArray();
            PrepareKeyboardSet();
            foreach (var language in AvailableKeyboardLayouts)
                language.OnSelect += Language_OnSelect;
        }

        public event EventHandler OnReturnKeyClick;
        public LanguageModel[] AvailableKeyboardLayouts { get; }
        public LanguageModel SelectedKeyboardLayout => AvailableKeyboardLayouts.FirstOrDefault(a => a.IsSelected);// { get; set; }//  = _AvailableKeyboardLayouts.First(a => a.Item1.IetfLanguageTag == "en");
        public Dictionary<string, KeyModel> KeyboardSet { get; private set; }

        public bool IsCapsLock { get; private set; }
        public bool IsShifted { get; private set; }
        public bool IsExtra { get; private set; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_LanguageSelector") is ToggleButton languageSelector)
                languageSelector.Checked += (sender, args) => DropDownButtonHelper.OpenDropDownMenu(sender);

            foreach (var button in Tips.GetVisualChildren(this).OfType<ButtonBase>().Where(a => a.DataContext is KeyModel))
                button.Click += Key_OnClick;

        }

        private void Key_OnClick(object sender, EventArgs e)
        {
            var model = (KeyModel)((FrameworkElement)sender).DataContext;
            if (model.Id == KeyModel.KeyDefinition.KeyCode.VK_CAPITAL)
            {
                IsCapsLock = Equals(((ToggleButton)sender).IsChecked, true);
                OnPropertiesChanged(nameof(IsCapsLock));
            }
            else if (model.Id == KeyModel.KeyDefinition.KeyCode.VK_LSHIFT || model.Id == KeyModel.KeyDefinition.KeyCode.VK_RSHIFT)
            {
                IsShifted = Equals(((ToggleButton)sender).IsChecked, true);
                OnPropertiesChanged(nameof(IsShifted));
            }
            else if (model.Id == KeyModel.KeyDefinition.KeyCode.VK_EXTRA)
            {
                IsExtra = Equals(((ToggleButton)sender).IsChecked, true);
                OnPropertiesChanged(nameof(IsExtra));
            }
            else if (model.Id == KeyModel.KeyDefinition.KeyCode.VK_TAB)
            {
                var request =
                    new TraversalRequest(IsShifted ? FocusNavigationDirection.Previous : FocusNavigationDirection.Next)
                        {Wrapped = true};
                LinkedTextBox.MoveFocus(request);
                LinkedTextBox.SelectionLength = 0;
                return;
            }
            else if (model.Id == KeyModel.KeyDefinition.KeyCode.VK_RETURN)
            {
                OnReturnKeyClick?.Invoke(this, EventArgs.Empty);
            }
            else
                EditTextBox(model);


            if (LinkedTextBox != null)
                LinkedTextBox.SelectionLength = 0;
        }

        private void EditTextBox(KeyModel keyModel)
        {
            if (LinkedTextBox != null && !LinkedTextBox.IsReadOnly && LinkedTextBox.IsEnabled)
            {
                var cursorPosition = LinkedTextBox.SelectionStart;
                string newText = "";
                if (!keyModel.IsCommand)
                {
                    newText = keyModel.GetKeyText(IsCapsLock, IsShifted, IsExtra);
                    LinkedTextBox.Text = LinkedTextBox.Text.Substring(0, cursorPosition) + newText +
                                         LinkedTextBox.Text.Substring(cursorPosition + LinkedTextBox.SelectionLength);

                }
                else if (keyModel.Id == KeyModel.KeyDefinition.KeyCode.VK_LEFT)
                {
                    if (cursorPosition > 0) cursorPosition--;
                }
                else if (keyModel.Id == KeyModel.KeyDefinition.KeyCode.VK_RIGHT)
                {
                    if (cursorPosition < LinkedTextBox.Text.Length) cursorPosition++;
                }
                else if (keyModel.Id == KeyModel.KeyDefinition.KeyCode.VK_BACK)
                {
                    if (LinkedTextBox.SelectionLength > 0)
                    {
                        LinkedTextBox.Text = LinkedTextBox.Text.Substring(0, cursorPosition) +
                                             LinkedTextBox.Text.Substring(cursorPosition + LinkedTextBox.SelectionLength);
                    }
                    else if (cursorPosition > 0)
                    {
                        LinkedTextBox.Text = LinkedTextBox.Text.Substring(0, cursorPosition - 1) +
                                             LinkedTextBox.Text.Substring(cursorPosition);
                        cursorPosition--;
                    }
                }
                else if (keyModel.Id == KeyModel.KeyDefinition.KeyCode.VK_DELETE)
                {
                    if (LinkedTextBox.SelectionLength > 0)
                    {
                        LinkedTextBox.Text = LinkedTextBox.Text.Substring(0, cursorPosition) +
                                             LinkedTextBox.Text.Substring(cursorPosition + LinkedTextBox.SelectionLength);
                    }
                    else if (cursorPosition < LinkedTextBox.Text.Length)
                    {
                        LinkedTextBox.Text = LinkedTextBox.Text.Substring(0, cursorPosition) +
                                             LinkedTextBox.Text.Substring(cursorPosition + 1);
                    }
                }

                // Focused textbox & set selection
                if (!LinkedTextBox.IsFocused)
                    LinkedTextBox.Focus();
                LinkedTextBox.SelectionStart = cursorPosition + (newText ?? "").Length;
            }
        }

        private void Language_OnSelect(object sender, EventArgs e)
        {
            PrepareKeyboardSet(((LanguageModel)sender).Id);
        }

        private void PrepareKeyboardSet(string language = DefaultLanguage)
        {
            KeyboardSet = KeyModel.GetKeyboardSet(language);
            foreach (var item in AvailableKeyboardLayouts)
                item.IsSelected = item.Id == language;
            OnPropertiesChanged(nameof(KeyboardSet), nameof(AvailableKeyboardLayouts), nameof(SelectedKeyboardLayout));
        }

        //===========  INotifyPropertyChanged  =======================
        #region ===========  INotifyPropertyChanged  ===============
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region ============== Properties/Events  ===================
        public static readonly DependencyProperty LinkedTextBoxProperty = DependencyProperty.Register("LinkedTextBox",
            typeof(TextBox), typeof(VirtualKeyboard), new FrameworkPropertyMetadata(null));
        public TextBox LinkedTextBox
        {
            get => (TextBox)GetValue(LinkedTextBoxProperty);
            set => SetValue(LinkedTextBoxProperty, value);
        }
        //=======================
        public static readonly DependencyProperty BaseHslProperty = DependencyProperty.Register("BaseHsl",
            typeof(HSL), typeof(VirtualKeyboard), new FrameworkPropertyMetadata(null));
        public HSL BaseHsl
        {
            get => (HSL)GetValue(BaseHslProperty);
            set => SetValue(BaseHslProperty, value);
        }
        //=======================
        public static readonly DependencyProperty BaseFontSizeProperty = DependencyProperty.Register("BaseFontSize",
            typeof(double), typeof(VirtualKeyboard), new FrameworkPropertyMetadata(0.0));
        public double BaseFontSize
        {
            get => (double)GetValue(BaseFontSizeProperty);
            set => SetValue(BaseFontSizeProperty, value);
        }
        #endregion
    }
}
