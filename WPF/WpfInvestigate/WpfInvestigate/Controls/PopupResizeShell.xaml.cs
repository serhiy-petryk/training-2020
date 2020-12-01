﻿using System;
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

        public PopupResizeShell()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var resizeThumb = GetTemplateChild("PART_ResizeGrip") as Thumb;
            if (resizeThumb != null)
                resizeThumb.DragDelta += ThumbDragDelta;

            var root = GetTemplateChild("PART_Root") as Grid;
            foreach(var thumb in root.Children.OfType<Thumb>())
                thumb.DragDelta += ThumbDragDelta;
        }

        private void ThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            var popup = Tips.GetVisualParents(this).OfType<Popup>().FirstOrDefault();
            if (popup == null) return;

            if (double.IsNaN(popup.Width)) popup.Width = ActualWidth;
            if (double.IsNaN(popup.Height)) popup.Height = ActualHeight;
            if (!double.IsNaN(Width)) Width = double.NaN;
            if (!double.IsNaN(Height)) Height = double.NaN;
            if (Content is FrameworkElement content)
            {
                if (!double.IsNaN(content.Width)) content.Width = double.NaN;
                if (!double.IsNaN(content.Height)) content.Height = double.NaN;
            }

            if (thumb.Cursor == Cursors.SizeWE || thumb.Cursor == Cursors.SizeNWSE)
                popup.Width = Math.Min(MaxSize, Math.Max(popup.Width + e.HorizontalChange, MinSize));

            if (thumb.Cursor == Cursors.SizeNS || thumb.Cursor == Cursors.SizeNWSE)
                popup.Height = Math.Min(MaxSize, Math.Max(popup.Height + e.VerticalChange, MinSize));
        }
    }
}
