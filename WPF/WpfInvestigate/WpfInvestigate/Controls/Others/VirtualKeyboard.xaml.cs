using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfInvestigate.Controls.Helpers;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Interaction logic for VirtualKeyboard.xaml
    /// </summary>
    public partial class VirtualKeyboard : INotifyPropertyChanged
    {
        private const string DefaultLanguage = "EN";

        public VirtualKeyboard()
        {
            InitializeComponent();
            DataContext = this;
            AvailableKeyboardLayouts = CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures)
                .Where(a => KeyModel.KeyDefinition.LanguageDefinitions.Keys.Contains(a.IetfLanguageTag.ToUpper())).OrderBy(a => a.DisplayName)
                .Select(a => new LanguageModel(a.IetfLanguageTag)).ToArray();
            PrepareKeyboardSet();
        }

        public LanguageModel[] AvailableKeyboardLayouts { get; }
        public LanguageModel SelectedKeyboardLayout => AvailableKeyboardLayouts.FirstOrDefault(a => a.IsSelected);// { get; set; }//  = _AvailableKeyboardLayouts.First(a => a.Item1.IetfLanguageTag == "en");
        public Dictionary<string, KeyModel> KeyboardSet { get; private set; }

        public bool IsCapsLock { get; private set; }
        public bool IsShifted { get; private set; }
        public bool IsExtra { get; private set; }

        private void OpenDropDownMenu(object sender, RoutedEventArgs e) => DropDownButtonHelper.OpenDropDownMenu(sender);

        private void OnKeyClick(object sender, RoutedEventArgs e)
        {
            var model = ((FrameworkElement)sender).DataContext as KeyModel;
            if (model.Id == KeyModel.KeyDefinition.KeyCode.VK_CAPITAL)
            {
                IsCapsLock = !IsCapsLock;
                OnPropertiesChanged(new[] { nameof(IsCapsLock) });
            }
            else if (model.Id == KeyModel.KeyDefinition.KeyCode.VK_LSHIFT || model.Id == KeyModel.KeyDefinition.KeyCode.VK_RSHIFT)
            {
                IsShifted = !IsShifted;
                OnPropertiesChanged(new[] { nameof(IsShifted) });
            }
            else if (model.Id == KeyModel.KeyDefinition.KeyCode.VK_EXTRA)
            {
                IsExtra = !IsExtra;
                OnPropertiesChanged(new[] { nameof(IsExtra) });
            }
        }

        private void OnMenuItemClick(object sender, RoutedEventArgs e)
        {
            var selected = (LanguageModel)((MenuItem)sender).DataContext;
            PrepareKeyboardSet(selected.Id);
        }

        private void PrepareKeyboardSet(string language = DefaultLanguage)
        {
            KeyboardSet = KeyModel.GetKeyboardSet(language);
            foreach (var item in AvailableKeyboardLayouts)
                item.IsSelected = item.Id == language;
            OnPropertiesChanged(new[] { nameof(KeyboardSet), nameof(AvailableKeyboardLayouts), nameof(SelectedKeyboardLayout) });
        }

        //===========  INotifyPropertyChanged  =======================
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
