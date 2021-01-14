using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using WpfInvestigate.Common;
using WpfInvestigate.Common.ColorSpaces;
using WpfInvestigate.Helpers;

namespace WpfInvestigate.Controls
{
    public class MessageContent : Control
    {
        public enum MessageContentIcon { Question, Stop, Error, Warning, Info, Success }

        static MessageContent()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MessageContent), new FrameworkPropertyMetadata(typeof(MessageContent)));
            KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(MessageContent), new FrameworkPropertyMetadata(false));
            // FocusableProperty.OverrideMetadata(typeof(MessageContent), new FrameworkPropertyMetadata(false));
        }

        private static readonly Color _defaultBaseColor = ColorUtils.StringToColor("#FFE2EBF4");

        private static readonly string[] _iconColors = {"Primary", "Danger", "Danger", "Warning", "Info", "Success"};

        #region ============  Public Static Methods  =============
        public static string ShowDialog(string messageText, string caption, MessageContentIcon? icon = null, string[] buttons = null, bool isCloseButtonVisible = true)
        {
            var messageContent = CreateMessageContent(messageText, caption, icon, buttons, isCloseButtonVisible);
            var content = new ResizingControl
            {
                Content = messageContent,
                LimitPositionToPanelBounds = true
            };

            new DialogAdorner().ShowContentDialog(content);
            return messageContent.Result;
        }

        public static async Task<string> ShowAsync(string messageText, string caption, MessageContentIcon? icon = null, string[] buttons = null, bool isCloseButtonVisible = true)
        {
            var messageContent = CreateMessageContent(messageText, caption, icon, buttons, isCloseButtonVisible);
            var content = new ResizingControl
            {
                Content = messageContent,
                LimitPositionToPanelBounds = true
            };

            var adorner = new DialogAdorner();
            adorner.ShowContent(content);
            await adorner.WaitUntilClosed();
            return messageContent.Result;
        }

        public static void Show(string messageText, string caption, MessageContentIcon? icon = null, string[] buttons = null, bool isCloseButtonVisible = true)
        {
            var messageContent = CreateMessageContent(messageText, caption, icon, buttons, isCloseButtonVisible);
            var content = new ResizingControl
            {
                Content = messageContent,
                LimitPositionToPanelBounds = true
            };

            new DialogAdorner().ShowContent(content);
        }

        private static MessageContent CreateMessageContent(string messageText, string caption, MessageContentIcon? icon = null, string[] buttons = null, bool isCloseButtonVisible = true)
        {
            var messageContent = new MessageContent { MessageText = messageText, Caption = caption, IsCloseButtonVisible = isCloseButtonVisible };
            if (icon != null)
            {
                messageContent.Icon = Application.Current?.TryFindResource($"{icon.Value}Geometry") as Geometry;
                messageContent.BaseIconColor = Application.Current?.TryFindResource(_iconColors[(int)icon] + "Color") as Color?;
                if (messageContent.BaseIconColor.HasValue)
                    messageContent.BaseColor = messageContent.BaseIconColor.Value;
            }
            if (buttons != null)
                messageContent.Buttons = buttons;

            return messageContent;
        }
        #endregion

        // =================  Instance  ================
        public string Result { get; private set; }
        public string MessageText { get; set; }
        public string Caption { get; set; }
        public Geometry Icon { get; set; }
        public Color BaseColor { get; set; } = _defaultBaseColor;
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

        public Color IconColor => BaseIconColor.HasValue && BaseIconColor.Value != BaseColor
            ? BaseIconColor.Value
            : (Color) ColorHslBrush.Instance.Convert(BaseIconColor ?? BaseColor, typeof(Color), "+70%", null);

        private RelayCommand _cmdClickButton;

        private MessageContent()
        {
            DataContext = this;
            _cmdClickButton = new RelayCommand(OnButtonClick);

            var currentWindow = Application.Current?.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            if (currentWindow != null)
                MaxWidth = Math.Max(400, currentWindow.ActualWidth / 2);
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

            foreach (var content in _buttons ?? new string[0])
            {
                _buttonsArea.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                var button = new Button { Content = content, Command = _cmdClickButton, CommandParameter = content };
                Grid.SetColumn(button, _buttonsArea.ColumnDefinitions.Count - 1);
                _buttonsArea.Children.Add(button);
            }
        }

        private void UpdateUI()
        {

            /*var currentWindow = Application.Current?.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            if (currentWindow != null)
            {
                var newMaxWidth = Math.Max(400, currentWindow.ActualWidth * 0.9);
                if (MaxWidth < newMaxWidth)
                    MaxWidth = newMaxWidth;
            }*/

            if (_buttonsArea.Children.Count > 0)
            {
                var maxWidth = _buttonsArea.Children.OfType<ContentControl>().Max(c => ControlHelper.MeasureString((string)c.Content, c).Width) + 4.0;
                MinWidth = Math.Min(MaxWidth, (maxWidth + 2) * _buttonsArea.Children.Count);

                // First measure
                if (double.IsNaN(Width) && ActualWidth < (MaxWidth + MinWidth) / 2)
                    Width = (MaxWidth + MinWidth) / 2;

                var space = ActualWidth - MinWidth;
                var padding = Math.Min(20.0, space / (4 * _buttonsArea.Children.Count));
                var buttonWidth = maxWidth + 2 * padding;

                foreach (ButtonBase button in _buttonsArea.Children)
                    if (!Tips.AreEqual(button.Width, buttonWidth))
                        button.Width = buttonWidth;

                if (!Tips.AreEqual(padding, _buttonsArea.Margin.Left) || !Tips.AreEqual(padding, _buttonsArea.Margin.Right))
                    _buttonsArea.Margin = new Thickness(padding, 5, padding, 0);
            }
        }
        #region ===========  Properties  ==============
        public static readonly DependencyProperty FocusButtonStyleProperty = DependencyProperty.Register("FocusButtonStyle", typeof(Style), typeof(MessageContent), new FrameworkPropertyMetadata(null));
        public Style FocusButtonStyle
        {
            get => (Style)GetValue(FocusButtonStyleProperty);
            set => SetValue(FocusButtonStyleProperty, value);
        }
        #endregion
    }
}
