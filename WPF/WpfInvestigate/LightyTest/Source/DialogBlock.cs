using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace LightyTest.Source
{
    public class DialogBlock: ContentControl
    {
        /*private Action<FrameworkElement> _closedDelegate;

        public EventHandler AllDialogClosed;

        public EventHandler CompleteInitializeLightBox;

        static DialogBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogBlock), new FrameworkPropertyMetadata(typeof(DialogBlock)));
        }

        // protected override DependencyObject GetContainerForItemOverride() => new ContentControl();
        // protected override bool IsItemItsOwnContainerOverride(object item) => false;

        /// <summary>
        /// Displays the DialogBlock modelessly.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="content"></param>
        public static async void Show(UIElement owner, FrameworkElement content)
        {
            var adorner = GetAdorner(owner);
            if (adorner == null) { adorner = await CreateAdornerAsync(owner); }
            adorner.Root?.AddDialog(content);
        }

        /// <summary>
        /// Display DialogBlock asynchronously and modeless.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static async Task ShowAsync(UIElement owner, FrameworkElement content)
        {
            var adorner = GetAdorner(owner);
            if (adorner == null) { adorner = await CreateAdornerAsync(owner); }
            await adorner.Root?.AddDialogAsync(content);
        }

        /// <summary>
        /// Display the DialogBlock modally.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="content"></param>
        public static void ShowDialog(UIElement owner, FrameworkElement content)
        {
            var adorner = GetAdorner(owner);
            if (adorner == null) { adorner = CreateAdornerModal(owner); }

            var frame = new DispatcherFrame();
            adorner.Root.AllDialogClosed += (s, e) => { frame.Continue = false; };
            adorner.Root?.AddDialog(content);

            Dispatcher.PushFrame(frame);
        }

        protected static LightBoxAdorner GetAdorner(UIElement element)
        {
            // If it is a Window class, use the Content property.
            var win = element as Window;
            var target = win?.Content as UIElement ?? element;

            if (target == null) return null;
            var layer = AdornerLayer.GetAdornerLayer(target);
            if (layer == null) return null;

            var current = layer.GetAdorners(target)?.OfType<LightBoxAdorner>()?.SingleOrDefault();
            return current;
        }

        private static LightBoxAdorner CreateAdornerCore(UIElement element, LightBox lightbox)
        {
            // If it is a Window class, use the Content property.
            var win = element as Window;
            var target = win?.Content as UIElement ?? element;

            if (target == null) return null;
            var layer = AdornerLayer.GetAdornerLayer(target);
            if (layer == null) return null;

            // Since there is no Adorner for the dialog, create a new one and set and return it.
            var adorner = new LightBoxAdorner(target);
            adorner.SetRoot(lightbox);

            // If Adorner is set for Window, set margin to cancel Margin of Content element.
            if (win != null)
            {
                var content = win.Content as FrameworkElement;
                var margin = content.Margin;
                adorner.Margin = new Thickness(-margin.Left, -margin.Top, margin.Right, margin.Bottom);
                adorner.UseAdornedElementSize = false;
            }

            // If the target is Enable when the dialog is displayed, disable it only while the dialog is displayed.
            if (target.IsEnabled)
            {
                target.IsEnabled = false;
                lightbox.AllDialogClosed += (s, e) => { target.IsEnabled = true; };
            }
            // Added a process to remove Adorner when all dialogs are cleared
            lightbox.AllDialogClosed += (s, e) => { layer?.Remove(adorner); };
            layer.Add(adorner);
            return adorner;
        }

        protected static LightBoxAdorner CreateAdorner(UIElement element)
        {
            return CreateAdornerCore(element, new LightBox());
        }

        protected static Task<LightBoxAdorner> CreateAdornerAsync(UIElement element)
        {
            var tcs = new TaskCompletionSource<LightBoxAdorner>();

            var lightbox = new LightBox();
            var adorner = CreateAdornerCore(element, lightbox);
            lightbox.Loaded += (s, e) =>
            {
                // When executing animations in parallel or when there is no lightbox background animation,
                // this asynchronous method is completed immediately.
                if (lightbox.IsParallelInitialize ||
                    lightbox.InitializeStoryboard == null)
                {
                    tcs.SetResult(adorner);
                }
                else
                {
                    lightbox.CompleteInitializeLightBox += (_s, _e) =>
                    {
                        tcs.SetResult(adorner);
                    };
                }
            };

            return tcs.Task;
        }

        protected static LightBoxAdorner CreateAdornerModal(UIElement element)
        {
            var lightbox = new LightBox();

            var adorner = CreateAdornerCore(element, lightbox);
            if (!lightbox.IsParallelInitialize)
            {
                var frame = new DispatcherFrame();
                lightbox.CompleteInitializeLightBox += (s, e) =>
                {
                    frame.Continue = false;
                };

                Dispatcher.PushFrame(frame);
            }

            return adorner;
        }

        #region Dialog display related processing
        /// <summary>
        /// FrameworkElement passed as an argument is added to the displayed dialog item.
        /// </summary>
        /// <param name="dialog"></param>
        protected void AddDialog(FrameworkElement dialog)
        {
            var animation = this.OpenStoryboard;
            dialog.Loaded += (sender, args) =>
            {
                var container = this.ContainerFromElement(dialog) as FrameworkElement;
                container.Focus();

                // Prevent MouseLeftButtonDown event in lightbox from bubbling up when CloseOnClickBackground is enabled.
                container.MouseLeftButtonDown += (s, e) => { e.Handled = true; };

                var transform = new TransformGroup();
                transform.Children.Add(new ScaleTransform());
                transform.Children.Add(new SkewTransform());
                transform.Children.Add(new RotateTransform());
                transform.Children.Add(new TranslateTransform());
                container.RenderTransform = transform;
                container.RenderTransformOrigin = new Point(0.5, 0.5);

                animation?.BeginAsync(container);
            };

            // Set content
            this.Content = dialog;

            // For the added dialog, set the handler for ApplicationCommands.Close command.
            dialog.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, async (s, e) =>
            {
                await this.RemoveDialogAsync(dialog);
            }));

            // Set a handler for ApplicationCommands.Close command in ItemsControl.
            // (ItemsContainer In order to send it a Close command so that it can be closed.)
            var parent = dialog.Parent as FrameworkElement;
            parent.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, async (s, e) =>
            {
                await this.RemoveDialogAsync(e.Parameter as FrameworkElement);
            }));

            this.InvalidateVisual();
        }

        protected async Task<bool> AddDialogAsync(FrameworkElement dialog)
        {
            var tcs = new TaskCompletionSource<bool>();

            var closedHandler = new Action<FrameworkElement>((d) => { });
            closedHandler = new Action<FrameworkElement>((d) =>
            {
                if (d == dialog)
                {
                    tcs.SetResult(true);
                    this._closedDelegate -= closedHandler;
                }
            });
            this._closedDelegate += closedHandler;

            this.AddDialog(dialog);

            return await tcs.Task;
        }

        protected async Task RemoveDialogAsync(FrameworkElement dialog)
        {
            var index = this.Items.IndexOf(dialog);
            var count = this.Items.Count;

            if (this.IsParallelDispose)
            {
                var _ = this.DestroyDialogAsync(dialog);
            }
            else
            {
                await this.DestroyDialogAsync(dialog);
            }

            if (index != -1 && count == 1)
            {
                await this.DestroyAdornerAsync();
            }

            this._closedDelegate?.Invoke(dialog);
        }
        #endregion

        #region Various methods to execute Animation related Storyboard

        public override async void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            // Added a process to delete Adorner by clicking the background
            if (CloseOnClickBackground)
            {
                MouseLeftButtonDown += (s, e) =>
                {
                    foreach (FrameworkElement item in Items.Cast<object>().ToList())
                        // ToList - prevent error: 'Collection was modified; enumeration operation may not execute.'
                        ApplicationCommands.Close.Execute(item, null);
                };
            }
            await InitializeAdornerAsync();
        }

        protected async Task InitializeAdornerAsync()
        {
            var animation = this.InitializeStoryboard;
            await animation.BeginAsync(this);
            this.CompleteInitializeLightBox?.Invoke(this, null);
        }

        protected async Task<bool> DestroyAdornerAsync()
        {
            var ret = await this.DisposeStoryboard.BeginAsync(this);
            // Issues an event asking you to delete this Adorner.
            this.AllDialogClosed?.Invoke(this, null);
            return ret;
        }

        protected async Task<bool> DestroyDialogAsync(FrameworkElement item)
        {
            var container = this.ContainerFromElement(item) as FrameworkElement;
            await this.CloseStoryboard.BeginAsync(container);
            this.Items.Remove(item);
            return true;
        }

        #endregion

        #region Animation related properties

        public bool IsParallelInitialize
        {
            get { return (bool)GetValue(IsParallelInitializeProperty); }
            set { SetValue(IsParallelInitializeProperty, value); }
        }
        // Using a DependencyProperty as the backing store for IsParallelInitialize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsParallelInitializeProperty =
            DependencyProperty.Register("IsParallelInitialize", typeof(bool), typeof(DialogBlock), new PropertyMetadata(false));

        public bool IsParallelDispose
        {
            get { return (bool)GetValue(IsParallelDisposeProperty); }
            set { SetValue(IsParallelDisposeProperty, value); }
        }
        // Using a DependencyProperty as the backing store for IsParallelDispose.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsParallelDisposeProperty =
            DependencyProperty.Register("IsParallelDispose", typeof(bool), typeof(DialogBlock), new PropertyMetadata(false));

        public Storyboard OpenStoryboard
        {
            get { return (Storyboard)GetValue(OpenStoryboardProperty); }
            set { SetValue(OpenStoryboardProperty, value); }
        }
        // Using a DependencyProperty as the backing store for OpenStoryboard.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpenStoryboardProperty =
            DependencyProperty.Register("OpenStoryboard", typeof(Storyboard), typeof(DialogBlock), new PropertyMetadata(null));

        public Storyboard CloseStoryboard
        {
            get { return (Storyboard)GetValue(CloseStoryboardProperty); }
            set { SetValue(CloseStoryboardProperty, value); }
        }
        // Using a DependencyProperty as the backing store for CloseStoryboard.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseStoryboardProperty =
            DependencyProperty.Register("CloseStoryboard", typeof(Storyboard), typeof(DialogBlock), new PropertyMetadata(null));

        public Storyboard InitializeStoryboard
        {
            get { return (Storyboard)GetValue(InitializeStoryboardProperty); }
            set { SetValue(InitializeStoryboardProperty, value); }
        }
        // Using a DependencyProperty as the backing store for InitializeStoryboard.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InitializeStoryboardProperty =
            DependencyProperty.Register("InitializeStoryboard", typeof(Storyboard), typeof(DialogBlock), new PropertyMetadata(null));

        public Storyboard DisposeStoryboard
        {
            get { return (Storyboard)GetValue(DisposeStoryboardProperty); }
            set { SetValue(DisposeStoryboardProperty, value); }
        }
        // Using a DependencyProperty as the backing store for DisposeStoryboard.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisposeStoryboardProperty =
            DependencyProperty.Register("DisposeStoryboard", typeof(Storyboard), typeof(DialogBlock), new PropertyMetadata(null));

        public bool CloseOnClickBackground
        {
            get { return (bool)GetValue(CloseOnClickBackgroundProperty); }
            set { SetValue(CloseOnClickBackgroundProperty, value); }
        }
        // Using a DependencyProperty as the backing store for CloseOnClickBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseOnClickBackgroundProperty =
            DependencyProperty.Register("CloseOnClickBackground", typeof(bool), typeof(DialogBlock), new PropertyMetadata(true));

        #endregion*/
    }
}
