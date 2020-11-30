using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Interaction logic for PopupResizeShell.xaml
    /// </summary>
    public partial class PopupResizeShell : ContentControl
    {
        private const int MaxSize = 1000;
        private const int MinSize = 50;

        private Popup _popup;
        private Thumb _resizeThumb;

        public PopupResizeShell()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _resizeThumb = GetTemplateChild("PART_ResizeThumb") as Thumb;
            if (_resizeThumb != null)
            {
                _resizeThumb.DragDelta += ThumbDragDelta;
            }

        }
        private void ThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            var t = sender as Thumb;
            var popup = Tips.GetVisualParents(this).OfType<Popup>().FirstOrDefault();
            if (popup == null) return;

            if (t.Cursor == Cursors.SizeWE || t.Cursor == Cursors.SizeNWSE)
                popup.Width = Math.Min(MaxSize, Math.Max(popup.Width + e.HorizontalChange, MinSize));

            if (t.Cursor == Cursors.SizeNS || t.Cursor == Cursors.SizeNWSE)
                popup.Height = Math.Min(MaxSize, Math.Max(popup.Height + e.VerticalChange, MinSize));
            Debug.Print($"Resize: {e.HorizontalChange}, {e.VerticalChange}");
            Debug.Print($"Popup: {popup.Width}, {popup.Height}");
        }

        private void ThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            //This is called when the user
            //starts dragging the thumb
        }

        private void ThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            //This is called when the user
            //finishes dragging the thumb
        }
    }
}
