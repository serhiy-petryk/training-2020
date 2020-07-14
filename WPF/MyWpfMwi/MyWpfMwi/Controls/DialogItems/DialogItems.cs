// ===============================================================
// Based on the https://github.com/sourcechord/Lighty (MIT licence)
// ===============================================================

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace MyWpfMwi.Controls.DialogItems
{
    public class DialogItems : ItemsControl
    {
        private Action<FrameworkElement> _closedDelegate;

        public EventHandler AllDialogClosed;
        public EventHandler CompleteInitializeDialogItems;

        static DialogItems()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogItems), new FrameworkPropertyMetadata(typeof(DialogItems)));
        }
        public DialogItems()
        {
            if (CloseOnClickBackground)
                MouseLeftButtonDown += DialogItems_MouseLeftButtonDown;
        }

        protected override DependencyObject GetContainerForItemOverride() => new ContentControl();
        protected override bool IsItemItsOwnContainerOverride(object item) => false;

        /// <summary>
        /// Usage example: DialogItems.Show(owner, content, style, DialogItems.GetAfterCreationCallbackForMovableDialog(content, false));
        /// </summary>
        /// <param name="content"></param>
        /// <param name="closeOnClickBackground"></param>
        /// <returns></returns>
        public static Action<DialogItems> GetAfterCreationCallbackForMovableDialog(FrameworkElement content, bool closeOnClickBackground)
        {
            return dialogItems =>
            {
                content.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                {
                    dialogItems.CloseOnClickBackground = closeOnClickBackground;

                    // center content position
                    if (dialogItems.ItemsPresenter != null)
                        dialogItems.ItemsPresenter.Margin = new Thickness
                        {
                            Left = Math.Max(0, (dialogItems.ItemsPresenter.ActualWidth - content.ActualWidth) / 2),
                            Top = Math.Max(0, (dialogItems.ItemsPresenter.ActualHeight - content.ActualHeight) / 2)
                        };
                }));
            };
        }

        /// <summary>
        /// Displays the DialogItems modelessly.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="content"></param>
        /// <param name="afterCreationCallback"></param>
        public static async void Show(UIElement owner, FrameworkElement content, Style style, Action<DialogItems> afterCreationCallback = null)
        {
            owner = owner ?? Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            var adorner = GetAdorner(owner);
            if (adorner == null)
                adorner = await CreateAdornerAsync(owner, style);

            if (adorner.Child != null && adorner.Child is DialogItems)
                ((DialogItems)adorner.Child).AddDialog(content);

            afterCreationCallback?.Invoke((DialogItems)adorner.Child);
        }

        /// <summary>
        /// Display DialogItems asynchronously and modeless.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="content"></param>
        /// <param name="afterCreationCallback"></param>
        /// <returns></returns>
        public static async Task ShowAsync(UIElement owner, FrameworkElement content, Style style, Action<DialogItems> afterCreationCallback = null)
        {
            owner = owner ?? Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            var adorner = GetAdorner(owner);
            if (adorner == null)
                adorner = await CreateAdornerAsync(owner, style);

            if (adorner.Child != null && adorner.Child is DialogItems)
            {
                var task = ((DialogItems) adorner.Child).AddDialogAsync(content);
                afterCreationCallback?.Invoke((DialogItems)adorner.Child);
                await task;
            }
        }

        /// <summary>
        /// Display the DialogItems modally.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="content"></param>
        /// <param name="afterCreationCallback"></param>
        public static void ShowDialog(UIElement owner, FrameworkElement content, Style style, Action<DialogItems> afterCreationCallback = null)
        {
            owner = owner ?? Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            var adorner = GetAdorner(owner);
            if (adorner == null)
                adorner = CreateAdornerModal(owner, style);

            var frame = new DispatcherFrame();
            ((DialogItems) adorner.Child).AllDialogClosed += (s, e) => frame.Continue = false;
            ((DialogItems) adorner.Child).AddDialog(content);

            afterCreationCallback?.Invoke((DialogItems)adorner.Child);

            Dispatcher.PushFrame(frame);
        }

        protected static AdornerControl GetAdorner(UIElement element)
        {
            // If it is a Window class, use the Content property.
            var win = element as Window;
            var target = win?.Content as UIElement ?? element;

            if (target == null)
                return null;
            var layer = AdornerLayer.GetAdornerLayer(target);
            if (layer == null)
                return null;

            var current = layer.GetAdorners(target)?.OfType<AdornerControl>()?.SingleOrDefault();
            return current;
        }

        private static AdornerControl CreateAdornerCore(UIElement element, DialogItems dialogItems)
        {
            // If it is a Window class, use the Content property.
            var win = element as Window;
            var target = win?.Content as UIElement ?? element;

            if (target == null)
                return null;
            var layer = AdornerLayer.GetAdornerLayer(target);
            if (layer == null)
                return null;

            // Since there is no Adorner for the dialog, create a new one and set and return it.
            var adorner = new AdornerControl(target);
            adorner.Child = dialogItems;

            // If Adorner is set for Window, set margin to cancel Margin of Content element.
            if (win != null)
            {
                var content = win.Content as FrameworkElement;
                var margin = content.Margin;
                // adorner.Margin = new Thickness(-margin.Left, -margin.Top, margin.Right, margin.Bottom);
                // adorner.UseAdornedElementSize = false;
            }

            // If the target is Enable when the dialog is displayed, disable it only while the dialog is displayed.
            if (target.IsEnabled)
            {
                target.IsEnabled = false;
                dialogItems.AllDialogClosed += (s, e) => target.IsEnabled = true;
            }
            // Added a process to remove Adorner when all dialogs are cleared
            dialogItems.AllDialogClosed += (s, e) => layer.Remove(adorner);
            // Microsoft bug???: CloseOnClickBackground is changing after "layer.Add(adorner)" in MultipleLightBoxWindow example
            var temp = dialogItems.CloseOnClickBackground;
            layer.Add(adorner);
            dialogItems.CloseOnClickBackground = temp;
            return adorner;
        }

        protected static Task<AdornerControl> CreateAdornerAsync(UIElement element, Style style)
        {
            var tcs = new TaskCompletionSource<AdornerControl>();
            var dialogItems = new DialogItems();
            if (style != null)
                dialogItems.Style = style;
            var adorner = CreateAdornerCore(element, dialogItems);

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                // When executing animations in parallel or when there is no dialogitems background animation,
                // this asynchronous method is completed immediately.
                if (dialogItems.IsParallelInitialize || dialogItems.InitializeStoryboard == null)
                    tcs.SetResult(adorner);
                else
                    dialogItems.CompleteInitializeDialogItems += (s, e) => tcs.SetResult(adorner);
            }));
            return tcs.Task;
        }

        protected static AdornerControl CreateAdornerModal(UIElement element, Style style)
        {
            var dialogItems = new DialogItems();
            if (style != null)
                dialogItems.Style = style;
            var adorner = CreateAdornerCore(element, dialogItems);

            if (!dialogItems.IsParallelInitialize)
            {
                var frame = new DispatcherFrame();
                dialogItems.CompleteInitializeDialogItems += (s, e) => frame.Continue = false;
                Dispatcher.PushFrame(frame);
            }

            return adorner;
        }

        private Panel _itemsHostPanel;
        public Panel ItemsHostPanel
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

        public ItemsPresenter ItemsPresenter => ItemsHostPanel?.TemplatedParent as ItemsPresenter;

        #region Dialog display related processing
        /// <summary>
        /// FrameworkElement passed as an argument is added to the displayed dialog item.
        /// </summary>
        /// <param name="dialog"></param>
        protected void AddDialog(FrameworkElement dialog)
        {
            dialog.Loaded += (sender, args) =>
            {
                var parent = dialog.Parent as FrameworkElement;
                var animation = OpenStoryboard;
                var container = ContainerFromElement(dialog) as FrameworkElement;
                container.Focus();
                container.MouseLeftButtonDown += (s, e) => e.Handled = true;

                var transform = new TransformGroup();
                transform.Children.Add(new ScaleTransform());
                transform.Children.Add(new SkewTransform());
                transform.Children.Add(new RotateTransform());
                transform.Children.Add(new TranslateTransform());
                container.RenderTransform = transform;
                container.RenderTransformOrigin = new Point(0.5, 0.5);

                // For the added dialog, set the handler for ApplicationCommands.Close command.
                dialog.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, async (s, e) => await RemoveDialogAsync(dialog)));

                // Set a handler for ApplicationCommands.Close command in ItemsControl.
                // (ItemsContainer In order to send it a Close command so that it can be closed.)
                parent?.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, async (s, e) => await RemoveDialogAsync(e.Parameter as FrameworkElement)));

                animation?.BeginAsync(container);
            };

            // Add item
            Items.Add(dialog);
            InvalidateVisual();
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

            AddDialog(dialog);

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

        private static async void DialogItems_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dialogItems = (DialogItems)sender;
            var tasks = dialogItems.Items.Cast<FrameworkElement>().Select(dialogItems.RemoveDialogAsync);
            await Task.WhenAll(tasks);
            await dialogItems.DestroyAdornerAsync();
        }

        #endregion

        #region Various methods to execute Animation related Storyboard

        public override async void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            await InitializeAdornerAsync();
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
            MouseLeftButtonDown -= DialogItems_MouseLeftButtonDown;
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
            DependencyProperty.Register("CloseOnClickBackground", typeof(bool), typeof(DialogItems), new PropertyMetadata(true, CloseOnClickBackgroundChanged));
        private static void CloseOnClickBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dialogItems = (DialogItems)d;
            dialogItems.MouseLeftButtonDown -= DialogItems_MouseLeftButtonDown;
            // Added a process to delete Adorner by clicking the background
            if (Equals(e.NewValue, true))
                dialogItems.MouseLeftButtonDown += DialogItems_MouseLeftButtonDown;
        }
        #endregion
    }
}
