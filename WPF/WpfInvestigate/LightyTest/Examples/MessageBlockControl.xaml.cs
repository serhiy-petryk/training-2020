using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using LightyTest.Common;
using LightyTest.Source;

namespace LightyTest.Examples
{
    public partial class MessageBlockControl : UserControl
    {
        private MessageBlock Data { get; }
        private RelayCommand CmdClickButton { get; }

        private MessageBlockControl()
        {
            InitializeComponent();
        }
        public MessageBlockControl(MessageBlock data) : this()
        {
            Data = data;
            DataContext = Data;
            CmdClickButton = new RelayCommand(OnButtonClick);

            IconBox.Child = data.Icon;

            if (data.Buttons != null && data.Buttons.Length > 0)
            {
                Buttons.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                for (var k = 0; k < data.Buttons.Length; k++)
                {
                    Buttons.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                    var btn = new Button { Margin = new Thickness(8, 4, 8, 4), Content = data.Buttons[k] };
                    Grid.SetColumn(btn, k + 1);
                    Buttons.Children.Add(btn);
                    btn.Command = CmdClickButton;
                    btn.CommandParameter = data.Buttons[k];
                }
                Buttons.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            var currentWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            if (currentWindow != null)
                MaxWidth = Math.Max(400, currentWindow.ActualWidth / 2);
        }

        private void OnButtonClick(object parameter)
        {
            Data.Result = parameter?.ToString();
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
    }
}
