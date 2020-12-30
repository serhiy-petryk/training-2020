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
    public class MessageBlock : Control
    {
        public enum MessageBlockIcon { Question, Stop, Error, Warning, Info, Success }

        static MessageBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MessageBlock), new FrameworkPropertyMetadata(typeof(MessageBlock)));
            KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(MessageBlock), new FrameworkPropertyMetadata(false));
            // FocusableProperty.OverrideMetadata(typeof(MessageBlock), new FrameworkPropertyMetadata(false));
        }

        private static readonly Color _defaultBaseColor = ColorUtils.StringToColor("#FFE2EBF4");

        private static readonly string[] _iconColors = {"Primary", "Danger", "Danger", "Warning", "Info", "Success"};

        #region ============  Public Static Methods  =============
        public static MessageBlock CreateMessageBlock(string messageText, string caption, MessageBlockIcon? icon = null, string[] buttons = null, bool isCloseButtonVisible = true)
        {
            var messageBlock = new MessageBlock { MessageText = messageText, Caption = caption, IsCloseButtonVisible = isCloseButtonVisible };
            if (icon != null)
            {
                messageBlock.Icon = Application.Current?.TryFindResource($"{icon.Value}Geometry") as Geometry;
                messageBlock.BaseIconColor = Application.Current?.TryFindResource(_iconColors[(int)icon] + "Color") as Color?;
                if (messageBlock.BaseIconColor.HasValue)
                    messageBlock.BaseColor = messageBlock.BaseIconColor.Value;
            }
            if (buttons != null)
                messageBlock.Buttons = buttons;

            return messageBlock;
        }

        public static string ShowDialog(MessageBlock messageBlock)
        {
            var dialogItems = new DialogItems();
            dialogItems.ShowDialog(messageBlock);
            return messageBlock.Result;
        }
        public static async Task<string> ShowAsync(MessageBlock messageBlock)
        {
            var dialogItems = new DialogItems();
            await dialogItems.ShowAsync(messageBlock);
            return messageBlock.Result;
        }
        public static string Show(MessageBlock messageBlock)
        {
            var dialogItems = new DialogItems();
            dialogItems.Show(messageBlock);
            return messageBlock.Result;
        }

        public static string ShowDialog(string messageText, string caption, MessageBlockIcon? icon = null, string[] buttons = null, bool isCloseButtonVisible = true)
        {
            var messageBlock = CreateMessageBlock(messageText, caption, icon, buttons, isCloseButtonVisible);
            new DialogItems().ShowDialog(messageBlock);
            return messageBlock.Result;
        }

        public static async Task<string> ShowAsync(string messageText, string caption, MessageBlockIcon? icon = null, string[] buttons = null, bool isCloseButtonVisible = true)
        {
            var messageBlock = CreateMessageBlock(messageText, caption, icon, buttons, isCloseButtonVisible);
            await new DialogItems().ShowAsync(messageBlock);
            return messageBlock.Result;
        }

        public static string Show(string messageText, string caption, MessageBlockIcon? icon = null, string[] buttons = null, bool isCloseButtonVisible = true)
        {
            var messageBlock = CreateMessageBlock(messageText, caption, icon, buttons, isCloseButtonVisible);
            new DialogItems().Show(messageBlock);
            return messageBlock.Result;
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

        private MessageBlock()
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

            foreach (var thumb in Tips.GetVisualChildren(this).OfType<Thumb>())
            {
                thumb.DragStarted += Thumb_OnDragStarted;
                if (thumb.Name == "PART_HeaderMover")
                    thumb.DragDelta += MoveThumb_OnDragDelta;
                else
                    thumb.DragDelta += ResizeThumb_OnDragDelta;
            }

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
            ApplicationCommands.Close.Execute(null, ((DialogItems)Parent).Items[0] as FrameworkElement);
        }

        #region ==========  Moving && resizing  =========
        private void Thumb_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            if (!Focusable)
                Focus();
            e.Handled = true;
        }

        private void MoveThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var itemsPresenter = Tips.GetVisualParents(this).OfType<ItemsPresenter>().FirstOrDefault();
            var container = itemsPresenter == null ? null : VisualTreeHelper.GetParent(itemsPresenter) as FrameworkElement;
            if (itemsPresenter != null && container != null)
            {
                var newX = itemsPresenter.Margin.Left + e.HorizontalChange;
                if (newX + ActualWidth > container.ActualWidth)
                    newX = container.ActualWidth - ActualWidth;
                if (newX < 0) newX = 0;

                var newY = itemsPresenter.Margin.Top + e.VerticalChange;
                if (newY + ActualHeight > container.ActualHeight)
                    newY = container.ActualHeight - ActualHeight;
                if (newY < 0) newY = 0;

                itemsPresenter.Margin = new Thickness { Left = newX, Top = newY };
            }
            e.Handled = true;
        }

        private void ResizeThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = (Thumb)sender;
            var itemsPresenter = Tips.GetVisualParents(this).OfType<ItemsPresenter>().FirstOrDefault();

            if (thumb.HorizontalAlignment == HorizontalAlignment.Left)
                OnResizeLeft(e.HorizontalChange, itemsPresenter);
            else if (thumb.HorizontalAlignment == HorizontalAlignment.Right)
                OnResizeRight(e.HorizontalChange, itemsPresenter);

            if (thumb.VerticalAlignment == VerticalAlignment.Top)
                OnResizeTop(e.VerticalChange, itemsPresenter);
            else if (thumb.VerticalAlignment == VerticalAlignment.Bottom)
                OnResizeBottom(e.VerticalChange, itemsPresenter);

            e.Handled = true;
        }

        private void OnResizeLeft(double horizontalChange, FrameworkElement itemsPresenter)
        {
            if (itemsPresenter != null)
            {
                var change = Math.Min(horizontalChange, ActualWidth - MinWidth);
                if (itemsPresenter.Margin.Left + change < 0)
                    change = -itemsPresenter.Margin.Left;
                if ((ActualWidth - change) > MaxWidth)
                    change = ActualWidth - MaxWidth;

                if (!Tips.AreEqual(0.0, change))
                {
                    Width = ActualWidth - change;
                    itemsPresenter.Margin = new Thickness(itemsPresenter.Margin.Left + change, itemsPresenter.Margin.Top, 0, 0);
                }
            }
        }
        private void OnResizeTop(double verticalChange, FrameworkElement itemsPresenter)
        {
            if (itemsPresenter != null)
            {
                var change = Math.Min(verticalChange, ActualHeight - MinHeight);
                if (itemsPresenter.Margin.Top + change < 0)
                    change = -itemsPresenter.Margin.Top;
                if ((Height - change) > MaxHeight)
                    change = Height - MaxHeight;

                if (!Tips.AreEqual(0.0, change))
                {
                    Height = ActualHeight - change;
                    itemsPresenter.Margin = new Thickness(itemsPresenter.Margin.Left, itemsPresenter.Margin.Top + change, 0, 0);
                }
            }
        }
        private void OnResizeRight(double horizontalChange, FrameworkElement itemsPresenter)
        {
            var container = itemsPresenter == null ? null : VisualTreeHelper.GetParent(itemsPresenter) as FrameworkElement;
            var change = Math.Min(-horizontalChange, ActualWidth - MinWidth);

            if ((ActualWidth - change) > MaxWidth)
                change = ActualWidth - MaxWidth;
            if (container != null && (itemsPresenter.Margin.Left + ActualWidth - change) > container.ActualWidth)
                change = itemsPresenter.Margin.Left + ActualWidth - container.ActualWidth;

            if (!Tips.AreEqual(0.0, change))
                Width = ActualWidth - change;
        }
        private void OnResizeBottom(double verticalChange, FrameworkElement itemsPresenter)
        {
            var container = itemsPresenter == null ? null : VisualTreeHelper.GetParent(itemsPresenter) as FrameworkElement;
            var change = Math.Min(-verticalChange, ActualHeight - MinHeight);

            if ((ActualHeight - change) > MaxHeight)
                change = ActualHeight - MaxHeight;
            if (container != null && (itemsPresenter.Margin.Top + ActualHeight - change) > container.ActualHeight)
                change = itemsPresenter.Margin.Top + ActualHeight - container.ActualHeight;

            if (!Tips.AreEqual(0.0, change))
                Height = ActualHeight - change;
        }
        #endregion

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
        public static readonly DependencyProperty FocusButtonStyleProperty = DependencyProperty.Register("FocusButtonStyle", typeof(Style), typeof(MessageBlock), new FrameworkPropertyMetadata(null));
        public Style FocusButtonStyle
        {
            get => (Style)GetValue(FocusButtonStyleProperty);
            set => SetValue(FocusButtonStyleProperty, value);
        }
        #endregion
    }
}
