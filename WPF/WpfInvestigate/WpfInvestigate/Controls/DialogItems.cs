// ===============================================================
// Based on the https://github.com/sourcechord/Lighty (MIT licence)
// ===============================================================

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WpfInvestigate.Common;
using WpfInvestigate.Helpers;

namespace WpfInvestigate.Controls
{
    public class DialogItems : ItemsControl
    {
        static DialogItems()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogItems), new FrameworkPropertyMetadata(typeof(DialogItems)));
            KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(DialogItems), new FrameworkPropertyMetadata(false));
        }

        public EventHandler AllDialogClosed;
        public EventHandler CompleteInitializeDialogItems;

        public DialogItems()
        {
            ((INotifyCollectionChanged)Items).CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                    foreach (var item in e.NewItems.OfType<FrameworkElement>())
                        AddDialogInternal(item);
            };
        }

        #region ==========  Public methods ===========
        public async void Show(FrameworkElement content, UIElement host = null)
        {
            host = host ?? Application.Current?.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            await CreateAdornerAsync(host);
            if (content != null)
                Items.Add(content);
            ControlHelper.SetFocus(content);
        }
        public void ShowDialog(FrameworkElement content, UIElement host = null)
        {
            host = host ?? Application.Current?.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            CreateAdornerModal(host);
            var frame = new DispatcherFrame();
            AllDialogClosed += (s, e) => frame.Continue = false;
            if (content != null)
                Items.Add(content);
            ControlHelper.SetFocus(content);

            Dispatcher.PushFrame(frame);
        }
        public async Task ShowAsync(FrameworkElement content, UIElement host = null)
        {
            host = host ?? Application.Current?.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            await CreateAdornerAsync(host);
            if (content != null)
            {
                ControlHelper.SetFocus(content);
                await AddDialogAsync(content);
            }
        }

        /// <summary>
        /// FrameworkElement passed as an argument is added to the displayed dialog item.
        /// </summary>
        /// <param name="dialog"></param>
        public void AddDialog(FrameworkElement dialog)
        {
            Items.Add(dialog);
            InvalidateVisual();
        }
        #endregion

        private Action<FrameworkElement> _closedDelegate;

        protected override DependencyObject GetContainerForItemOverride() => new ContentControl {Focusable = false};
        protected override bool IsItemItsOwnContainerOverride(object item) => false;

        private Panel _itemsHostPanel;
        private Panel ItemsHostPanel
        {
            get
            {
                if (_itemsHostPanel == null)
                    _itemsHostPanel = typeof(DialogItems).InvokeMember("ItemsHost",
                        BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance, null, this,
                        null) as Panel;
                return _itemsHostPanel;
            }
        }

        #region Dialog display related processing

        private AdornerControl CreateAdornerModal(UIElement host)
        {
            var adorner = CreateAdornerCore(host);
            if (!IsParallelInitialize)
            {
                var frame = new DispatcherFrame();
                CompleteInitializeDialogItems += (s, e) => frame.Continue = false;
                Dispatcher.PushFrame(frame);
            }
            return adorner;
        }
        private Task<AdornerControl> CreateAdornerAsync(UIElement host)
        {
            var tcs = new TaskCompletionSource<AdornerControl>();
            var adorner = CreateAdornerCore(host);

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                // When executing animations in parallel or when there is no dialogitems background animation,
                // this asynchronous method is completed immediately.
                if (IsParallelInitialize || InitializeStoryboard == null)
                    tcs.SetResult(adorner);
                else
                    CompleteInitializeDialogItems += (s, e) => tcs.SetResult(adorner);
            }));
            return tcs.Task;
        }
        private AdornerControl CreateAdornerCore(UIElement host)
        {
            // If it is a Window class, use the Content property.
            var win = host as Window;
            var target = win?.Content as UIElement ?? host;
            if (target == null)
                return null;

            var layer = AdornerLayer.GetAdornerLayer(target);
            if (layer == null)
                return null;

            // Since there is no Adorner for the dialog, create a new one and set and return it.
            var adorner = new AdornerControl(target);
            adorner.Child = this;

            // If Adorner is set for Window, set margin to cancel Margin of Content element.
            if (win != null)
            {
                var content = win.Content as FrameworkElement;
                var margin = content.Margin;
                adorner.Margin = new Thickness(-margin.Left, -margin.Top, margin.Right, margin.Bottom);
                adorner.AdornerSize = AdornerControl.AdornerSizeType.Container;
            }

            // If the target is Enable when the dialog is displayed, disable it only while the dialog is displayed.
            if (target.IsEnabled)
            {
                target.IsEnabled = false;
                AllDialogClosed += (s, e) => target.IsEnabled = true;
            }
            // Added a process to remove Adorner when all dialogs are cleared
            AllDialogClosed += (s, e) => layer.Remove(adorner);
            layer.Add(adorner);
            return adorner;
        }

        private void AddDialogInternal(FrameworkElement content)
        {
            content.Loaded += (sender, args) =>
            {
                if (ItemsHostPanel.HorizontalAlignment == HorizontalAlignment.Left && ItemsHostPanel.VerticalAlignment == VerticalAlignment.Top)
                { // center the dialog content
                    var panel = ItemsHostPanel.TemplatedParent as FrameworkElement;
                    panel.Margin = new Thickness
                    {
                        Left = Math.Max(0, (panel.ActualWidth - content.ActualWidth) / 2),
                        Top = Math.Max(0, (panel.ActualHeight - content.ActualHeight) / 2)
                    };
                }

                var parent = content.Parent as FrameworkElement;
                var animation = OpenStoryboard;
                var container = ContainerFromElement(content) as FrameworkElement;
                container.Focus();
                container.MouseLeftButtonDown += (s, e) => e.Handled = true;

                // For the added dialog, set the handler for ApplicationCommands.Close command.
                content.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, async (s, e) => await RemoveDialogAsync(content)));

                // Set a handler for ApplicationCommands.Close command in ItemsControl.
                // (ItemsContainer In order to send it a Close command so that it can be closed.)
                parent?.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, async (s, e) => await RemoveDialogAsync(e.Parameter as FrameworkElement)));

                animation?.BeginAsync(container);
            };
        }

        protected async Task<bool> AddDialogAsync(FrameworkElement dialog)
        {
            var tcs = new TaskCompletionSource<bool>();
            var closedHandler = new Action<FrameworkElement>(d => { });
            closedHandler = d =>
            {
                if (d == dialog)
                {
                    tcs.SetResult(true);
                    _closedDelegate -= closedHandler;
                }
            };
            _closedDelegate += closedHandler;

            Items.Add(dialog);
            return await tcs.Task;
        }

        protected async Task RemoveDialogAsync(FrameworkElement dialog)
        {
            var index = Items.IndexOf(dialog);
            var count = Items.Count;

            if (IsParallelDispose)
            {
                var _ = DestroyDialogAsync(dialog);
            }
            else
                await DestroyDialogAsync(dialog);

            if (index != -1 && count == 1)
                await DestroyAdornerAsync();

            _closedDelegate?.Invoke(dialog);
        }

        #endregion

        #region Various methods to execute Animation related Storyboard

        public override async void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            await InitializeAdornerAsync();
        }

        protected override async void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (CloseOnClickBackground)
            {
                var tasks = Items.Cast<FrameworkElement>().Select(RemoveDialogAsync);
                await Task.WhenAll(tasks);
                await DestroyAdornerAsync();
            }
        }

        protected async Task InitializeAdornerAsync()
        {
            var animation = InitializeStoryboard;
            await animation.BeginAsync(this);
            CompleteInitializeDialogItems?.Invoke(this, null);
        }

        protected async Task<bool> DestroyAdornerAsync()
        {
            var ret = await DisposeStoryboard.BeginAsync(this);
            // Issues an event asking you to delete this Adorner.
            AllDialogClosed?.Invoke(this, null);
            return ret;
        }

        protected async Task<bool> DestroyDialogAsync(FrameworkElement item)
        {
            var container = ContainerFromElement(item) as FrameworkElement;
            await CloseStoryboard.BeginAsync(container);
            Items.Remove(item);
            return true;
        }

        #endregion

        #region Properties

        public bool IsParallelInitialize
        {
            get => (bool)GetValue(IsParallelInitializeProperty);
            set => SetValue(IsParallelInitializeProperty, value);
        }
        // Using a DependencyProperty as the backing store for IsParallelInitialize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsParallelInitializeProperty =
            DependencyProperty.Register("IsParallelInitialize", typeof(bool), typeof(DialogItems), new PropertyMetadata(false));

        public bool IsParallelDispose
        {
            get => (bool)GetValue(IsParallelDisposeProperty);
            set => SetValue(IsParallelDisposeProperty, value);
        }
        // Using a DependencyProperty as the backing store for IsParallelDispose.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsParallelDisposeProperty =
            DependencyProperty.Register("IsParallelDispose", typeof(bool), typeof(DialogItems), new PropertyMetadata(false));

        public Storyboard OpenStoryboard
        {
            get => (Storyboard)GetValue(OpenStoryboardProperty);
            set => SetValue(OpenStoryboardProperty, value);
        }
        // Using a DependencyProperty as the backing store for OpenStoryboard.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpenStoryboardProperty =
            DependencyProperty.Register("OpenStoryboard", typeof(Storyboard), typeof(DialogItems), new PropertyMetadata(null));

        public Storyboard CloseStoryboard
        {
            get => (Storyboard)GetValue(CloseStoryboardProperty);
            set => SetValue(CloseStoryboardProperty, value);
        }
        // Using a DependencyProperty as the backing store for CloseStoryboard.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseStoryboardProperty =
            DependencyProperty.Register("CloseStoryboard", typeof(Storyboard), typeof(DialogItems), new PropertyMetadata(null));

        public Storyboard InitializeStoryboard
        {
            get => (Storyboard)GetValue(InitializeStoryboardProperty);
            set => SetValue(InitializeStoryboardProperty, value);
        }
        // Using a DependencyProperty as the backing store for InitializeStoryboard.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InitializeStoryboardProperty =
            DependencyProperty.Register("InitializeStoryboard", typeof(Storyboard), typeof(DialogItems), new PropertyMetadata(null));

        public Storyboard DisposeStoryboard
        {
            get => (Storyboard)GetValue(DisposeStoryboardProperty);
            set => SetValue(DisposeStoryboardProperty, value);
        }
        // Using a DependencyProperty as the backing store for DisposeStoryboard.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisposeStoryboardProperty =
            DependencyProperty.Register("DisposeStoryboard", typeof(Storyboard), typeof(DialogItems), new PropertyMetadata(null));

        public bool CloseOnClickBackground
        {
            get => (bool)GetValue(CloseOnClickBackgroundProperty);
            set => SetValue(CloseOnClickBackgroundProperty, value);
        }
        // Using a DependencyProperty as the backing store for CloseOnClickBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseOnClickBackgroundProperty =
            DependencyProperty.Register("CloseOnClickBackground", typeof(bool), typeof(DialogItems), new PropertyMetadata(true));
        #endregion
    }
}
