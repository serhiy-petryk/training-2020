using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WpfSpLib.Common;
using WpfSpLib.Effects;
using WpfSpLib.Helpers;

namespace WpfSpLib.Controls
{
    public class DialogMessage : Control
    {
        public enum DialogMessageIcon { Question, Stop, Error, Warning, Info, Success }

        static DialogMessage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogMessage), new FrameworkPropertyMetadata(typeof(DialogMessage)));
            KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(DialogMessage), new FrameworkPropertyMetadata(false));
            FocusableProperty.OverrideMetadata(typeof(DialogMessage), new FrameworkPropertyMetadata(false));
        }

        private static readonly string[] _iconColors = {"Primary", "Danger", "Danger", "Warning", "Info", "Success"};

        #region ============  Public Static Methods  =============
        /// <summary>
        /// Open dialog message
        /// </summary>
        /// <param name="messageText"></param>
        /// <param name="caption"></param>
        /// <param name="icon"></param>
        /// <param name="buttons"></param>
        /// <param name="isCloseButtonVisible"></param>
        /// <param name="messageHost"></param>
        /// <returns>Dialog result</returns>
        public static string ShowDialog(string messageText, string caption = null, DialogMessageIcon? icon = null, string[] buttons = null, bool isCloseButtonVisible = true, FrameworkElement messageHost = null)
        {
            var resizingControl = CreateResizingControl(messageText, caption, icon, buttons, isCloseButtonVisible);
            var dialog = (DialogMessage)resizingControl.Content;
            new DialogAdorner(messageHost).ShowContentDialog(resizingControl);
            return dialog.Result;
        }

        public static async Task<string> ShowAsync(string messageText, string caption = null, DialogMessageIcon? icon = null, string[] buttons = null, bool isCloseButtonVisible = true, FrameworkElement messageHost = null)
        {
            var resizingControl = CreateResizingControl(messageText, caption, icon, buttons, isCloseButtonVisible);
            var adorner = new DialogAdorner(messageHost);
            var dialog = (DialogMessage)resizingControl.Content;
            await adorner.ShowContentAsync(resizingControl);
            await adorner.WaitUntilClosed();
            return dialog.Result;
        }

        public static void Show(string messageText, string caption = null, DialogMessageIcon? icon = null, string[] buttons = null, bool isCloseButtonVisible = true, FrameworkElement messageHost = null)
        {
            var resizingControl = CreateResizingControl(messageText, caption, icon, buttons, isCloseButtonVisible);
            new DialogAdorner(messageHost).ShowContent(resizingControl);
        }

        private static ResizingControl CreateResizingControl(string messageText, string caption, DialogMessageIcon? icon = null, string[] buttons = null, bool isCloseButtonVisible = true)
        {
            var dialogMessage = new DialogMessage { MessageText = messageText, Caption = caption, IsCloseButtonVisible = isCloseButtonVisible };
            if (icon != null)
            {
                dialogMessage.Icon = Application.Current?.TryFindResource($"{icon.Value}Geometry") as Geometry;
                dialogMessage.BaseIconColor = Application.Current?.TryFindResource(_iconColors[(int)icon] + "Color") as Color?;
                if (dialogMessage.BaseIconColor.HasValue)
                    dialogMessage.Background = new SolidColorBrush(dialogMessage.BaseIconColor.Value);
            }
            if (buttons != null)
                dialogMessage.Buttons = buttons;

            var content = new ResizingControl
            {
                Content = dialogMessage,
                LimitPositionToPanelBounds = true
            };
            CornerRadiusEffect.SetCornerRadius(content, CornerRadiusEffect.GetCornerRadius(dialogMessage));
            return content;
        }
        #endregion

        // =================  Instance  ================
        public string Result { get; private set; }
        public string MessageText { get; set; }
        public string Caption { get; set; }
        public Geometry Icon { get; set; }
        public Color? BaseIconColor { get; set; }
        public bool IsCloseButtonVisible { get; set; } = true;

        private Grid _buttonsArea;

        private IEnumerable<string> _buttons;
        private IEnumerable<string> Buttons
        {
            get => _buttons;
            set
            {
                _buttons = value;
                RefreshButtons();
            }
        }

        public Color IconColor => BaseIconColor.HasValue && BaseIconColor.Value != Tips.GetActualBackgroundColor(this)
            ? BaseIconColor.Value
            : (Color) ColorHslBrush.Instance.Convert(BaseIconColor ?? Tips.GetActualBackgroundColor(this), typeof(Color), "+70%", null);

        private RelayCommand _cmdClickButton;

        private DialogMessage()
        {
            _cmdClickButton = new RelayCommand(OnButtonClick);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _buttonsArea = GetTemplateChild("PART_ButtonsArea") as Grid;
            RefreshButtons();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateUI();
        }

        private void OnButtonClick(object parameter)
        {
            Result = parameter?.ToString();
            ApplicationCommands.Close.Execute(null, this);
        }

        private void RefreshButtons()
        {
            if (_buttonsArea == null) return;

            _buttonsArea.Children.Clear();
            _buttonsArea.ColumnDefinitions.Clear();

            if ((_buttons ?? new string[0]).Any())
            {
                MinHeight = 120;
                foreach (var content in _buttons)
                {
                    _buttonsArea.ColumnDefinitions.Add(new ColumnDefinition
                        { Width = new GridLength(1, GridUnitType.Star) });
                    var button = new Button { Content = content, Command = _cmdClickButton, CommandParameter = content };
                    Grid.SetColumn(button, _buttonsArea.ColumnDefinitions.Count - 1);
                    _buttonsArea.Children.Add(button);
                }
            }
            else
                MinHeight = 85;
        }

        private bool _isFirst = true;
        private bool _isUpdatingUI = false;
        private async void UpdateUI()
        {
            if (_isUpdatingUI) return;
            _isUpdatingUI = true;

            var noButtons = _buttonsArea.Children.Count == 0;
            var buttonBaseWidth = noButtons ? 0 : _buttonsArea.Children.OfType<ContentControl>().Max(c => ControlHelper.MeasureString((string)c.Content, c).Width) + 8.0;

            // First measure
            if (_isFirst)
            {
                _isFirst = false;

                var panel = Tips.GetVisualParents(this).OfType<Panel>().FirstOrDefault(p => p.ActualHeight > (ActualHeight + 0.5) || p.ActualWidth > (ActualWidth + 0.5));
                if (panel != null)
                {
                    MaxWidth = panel.ActualWidth;
                    MaxHeight = panel.ActualHeight;
                }

                var allButtonWidth = buttonBaseWidth * _buttonsArea.Children.Count * 2;
                var startWidth = Math.Max(allButtonWidth, Math.Min(ActualWidth + 1, MaxWidth / 2));
                Width = Math.Round(Math.Min(MaxWidth, startWidth));
                await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render).Task;
            }

            if (!noButtons)
            {
                MinWidth = Math.Max(100, Math.Min(MaxWidth, (buttonBaseWidth + 2) * _buttonsArea.Children.Count));
                var space = ActualWidth - MinWidth;
                var padding = Math.Min(20.0, space / (4 * _buttonsArea.Children.Count));
                var buttonWidth = buttonBaseWidth + 2 * padding;

                foreach (ButtonBase button in _buttonsArea.Children)
                    if (!Tips.AreEqual(button.Width, buttonWidth))
                        button.Width = buttonWidth;

                if (!Tips.AreEqual(padding, _buttonsArea.Margin.Left) ||
                    !Tips.AreEqual(padding, _buttonsArea.Margin.Right))
                    _buttonsArea.Margin = new Thickness(padding, 5, padding, 10);
            }

            await Dispatcher.InvokeAsync(() => _isUpdatingUI = false, DispatcherPriority.Render);
        }

        #region ===========  Properties  ==============
        public static readonly DependencyProperty FocusButtonStyleProperty = DependencyProperty.Register("FocusButtonStyle", typeof(Style), typeof(DialogMessage), new FrameworkPropertyMetadata(null));
        public Style FocusButtonStyle
        {
            get => (Style)GetValue(FocusButtonStyleProperty);
            set => SetValue(FocusButtonStyleProperty, value);
        }
        #endregion
    }
}
