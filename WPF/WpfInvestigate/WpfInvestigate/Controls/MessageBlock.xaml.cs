using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using WpfInvestigate.Common;
using WpfInvestigate.Common.ColorSpaces;
using WpfInvestigate.Controls.Helpers;

namespace WpfInvestigate.Controls
{
    public partial class MessageBlock : UserControl
    {
        public enum MessageBlockIcon { Question, Stop, Error, Warning, Info, Success }

        // static MessageBlock() => DefaultStyleKeyProperty.OverrideMetadata(typeof(MessageBlock), new FrameworkPropertyMetadata(typeof(MessageBlock)));

        private static readonly Color _defaultBaseColor = ColorUtils.StringToColor("#FFE2EBF4");

        private static readonly string[] _iconColors = {"Primary", "Danger", "Danger", "Warning", "Info", "Success"};

        public static string Show(string messageText, string caption, MessageBlockIcon? icon = null, string[] buttons = null)
        {
            var style = Application.Current.TryFindResource("MovableDialogStyle") as Style;
            var iconGeometry = icon == null ? null : Application.Current.TryFindResource($"{icon.Value}Geometry") as Geometry;
            var iconColor = icon == null ? null : Application.Current.TryFindResource(_iconColors[(int) icon] + "Color") as Color?;

            // var content = new MessageBlock(null, messageText, caption, iconGeometry, iconColor, buttons);
            var content = new MessageBlock(iconColor, messageText, caption, iconGeometry, iconColor, buttons);
            DialogItems.DialogItems.ShowDialog(null, content, style, DialogItems.DialogItems.GetAfterCreationCallbackForMovableDialog(content, true));
            return content._result;
        }

        // =================  Instance  ================
        public string MessageText { get; private set; }
        public string Caption { get; private set; }
        public Geometry Icon { get; private set; }
        public Color BaseColor { get; private set; }
        public Color? BaseIconColor { get; private set; }

        private string[] Buttons
        {
            set
            {
                ButtonsArea.Children.Clear();
                ButtonsArea.ColumnDefinitions.Clear();

                if (value != null && value.Length > 0)
                {
                    for (var k = 0; k < value.Length; k++)
                    {
                        ButtonsArea.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        // var btn = new Button { Content = value[k], Padding = new Thickness(0) };
                        var btn = new Button { Content = value[k], Style = Resources["MessageBoxButtonStyle"] as Style };
                        Grid.SetColumn(btn, k);
                        ButtonsArea.Children.Add(btn);
                        btn.Command = _cmdClickButton;
                        btn.CommandParameter = value[k];
                    }
                }

            }
        }

        public Color IconColor => BaseIconColor.HasValue && BaseIconColor.Value != BaseColor
            ? BaseIconColor.Value
            : (Color) ColorHslBrush.Instance.Convert(BaseIconColor ?? BaseColor, typeof(Color), "+70%", null);

        private string _result;
        private RelayCommand _cmdClickButton { get; }

        private MessageBlock()
        {
            InitializeComponent();
            DataContext = this;
            _cmdClickButton = new RelayCommand(OnButtonClick);

            var currentWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            if (currentWindow != null)
                MaxWidth = Math.Max(400, currentWindow.ActualWidth / 2);
        }
        private MessageBlock(Color? baseColor, string messageText, string caption, Geometry icon, Color? iconColor, string[] buttons) : this()
        {
            BaseColor = baseColor ?? _defaultBaseColor;
            BaseIconColor = iconColor;
            MessageText = messageText;
            Caption = caption;
            Icon = icon;
            Buttons = buttons;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateUI();
        }

        private void OnButtonClick(object parameter)
        {
            _result = parameter?.ToString();
            ApplicationCommands.Close.Execute(null, ((DialogItems.DialogItems)Parent).Items[0] as FrameworkElement);
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

        private void UpdateUI()
        {

            /*var currentWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            if (currentWindow != null)
            {
                var newMaxWidth = Math.Max(400, currentWindow.ActualWidth * 0.9);
                if (MaxWidth < newMaxWidth)
                    MaxWidth = newMaxWidth;
            }*/

            if (ButtonsArea.Children.Count > 0)
            {
                var maxWidth = ButtonsArea.Children.OfType<ContentControl>().Max(c => ControlHelper.MeasureString((string)c.Content, c).Width) + 4.0;
                MinWidth = Math.Min(MaxWidth, (maxWidth + 2) * ButtonsArea.Children.Count);

                // First measure
                if (double.IsNaN(Width) && ActualWidth < (MaxWidth + MinWidth) / 2)
                    Width = (MaxWidth + MinWidth) / 2;

                var space = ActualWidth - MinWidth;
                var padding = Math.Min(20.0, space / (4 * ButtonsArea.Children.Count));
                var buttonWidth = maxWidth + 2 * padding;

                foreach (ButtonBase button in ButtonsArea.Children)
                    if (!Tips.AreEqual(button.Width, buttonWidth))
                        button.Width = buttonWidth;

                if (!Tips.AreEqual(padding, ButtonsArea.Margin.Left) || !Tips.AreEqual(padding, ButtonsArea.Margin.Right))
                    ButtonsArea.Margin = new Thickness(padding, 5, padding, 0);
            }
        }
    }
}
