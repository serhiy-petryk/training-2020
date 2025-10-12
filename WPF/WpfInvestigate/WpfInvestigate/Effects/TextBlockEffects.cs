using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfInvestigate.Effects
{
    // Based on from https://blog.scottlogic.com/2011/01/31/automatically-showing-tooltips-on-a-trimmed-textblock-silverlight-wpf.html
    // usage:
    // 1. controls:TextBlockExt.AutoTooltipTarget:
    // <Thumb x:Name="MoveThumb" Height="30" VerticalAlignment="Top" Opacity="0" controls:TextBlockExt.AutoTooltipTarget="{Binding ElementName=Header}" ...
    // <Grid IsHitTestVisible="False">
    //      <TextBlock x:Name="Header" Grid.Column="1" TextTrimming="CharacterEllipsis" TextWrapping="NoWrap" Text="{TemplateBinding Title}"/>
    // (original but not here) 2. controls:TextBlockExt.AutoTooltip:
    // <TextBlock Text="The rain in Spain stays mainly in the plain" util:TextBlockUtils.AutoTooltip="True"/>

    public class TextBlockEffects
    {
        /// <summary>
        /// Gets the value of the AutoTooltipProperty dependency property
        /// </summary>
        public static bool GetAutoTooltipTarget(DependencyObject obj) => (bool)obj.GetValue(AutoTooltipTargetProperty);

        /// <summary>
        /// Sets the value of the AutoTooltipProperty dependency property
        /// </summary>
        public static void SetAutoTooltipTarget(DependencyObject obj, bool value) => obj.SetValue(AutoTooltipTargetProperty, value);

        /// <summary>
        /// Identified the attached AutoTooltip property. When true, this will set the TextBlock TextTrimming
        /// property to WordEllipsis, and display a tooltip with the full text whenever the text is trimmed.
        /// </summary>
        public static readonly DependencyProperty AutoTooltipTargetProperty = DependencyProperty.RegisterAttached("AutoTooltipTarget", typeof(TextBlock), typeof(TextBlockEffects), new PropertyMetadata(null, OnAutoTooltipTargetPropertyChanged));

        private static void OnAutoTooltipTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                var textBlock = e.NewValue as TextBlock;
                element.MouseEnter -= OnElementMouseEnter;
                if (textBlock != null)
                    element.MouseEnter += OnElementMouseEnter;

                void OnElementMouseEnter(object sender, System.Windows.Input.MouseEventArgs args)
                {
                    if (textBlock != null)
                    {
                        textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
                        ComputeAutoTooltip(textBlock, d);
                    }
                }
            }
        }

        /// <summary>
        /// Assigns the ToolTip for the given TextBlock based on whether the text is trimmed
        /// </summary>
        private static void ComputeAutoTooltip(TextBlock textBlock, DependencyObject toolTipPlaceHolder = null)
        {
            var isTextTrimmed = IsTextTrimmed(textBlock);
            ToolTipService.SetToolTip(toolTipPlaceHolder ?? textBlock, isTextTrimmed ? textBlock.Text : null);
        }

        public static bool IsTextTrimmedx(TextBlock textBlock)
        {
            // From https://stackoverflow.com/questions/1041820/how-can-i-determine-if-my-textblock-text-is-being-trimmed
            var typeface = new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch);
            var formattedText = new FormattedText(textBlock.Text, System.Threading.Thread.CurrentThread.CurrentCulture,
                textBlock.FlowDirection, typeface, textBlock.FontSize, textBlock.Foreground,
                VisualTreeHelper.GetDpi(textBlock).PixelsPerDip) { Trimming = TextTrimming.None };

            if (textBlock.TextWrapping == TextWrapping.NoWrap)
            {
                formattedText.MaxTextHeight = textBlock.DesiredSize.Height;
                Debug.Print($"IsTextTrimmed: { textBlock.ActualWidth}, {formattedText.Width}, {textBlock.ActualWidth < formattedText.Width}");
                return (textBlock.ActualWidth - textBlock.Padding.Left - textBlock.Padding.Right) < (textBlock.UseLayoutRounding ? Math.Round(formattedText.Width) : formattedText.Width); // 0.5d: for UseLayoutRounding = true
                // return textBlock.DesiredSize.Width < (formattedText.Width + 0.5d); // 0.5d: for UseLayoutRounding = true
            }
            else
            {
                formattedText.MaxTextWidth = textBlock.DesiredSize.Width;
                Debug.Print($"IsTextTrimmed: { textBlock.ActualHeight}, {formattedText.Height}, {textBlock.ActualHeight < formattedText.Height}");
                return (textBlock.ActualHeight - textBlock.Padding.Top - textBlock.Padding.Bottom) < formattedText.Height;
//                return textBlock.DesiredSize.Height < formattedText.Height;
            }

            // When the maximum text width of the FormattedText instance is set to the actual
            // width of the textBlock, if the textBlock is being trimmed to fit then the formatted
            // text will report a larger height than the textBlock. Should work whether the
            // textBlock is single or multi-line.
            // The "formattedText.MinWidth > formattedText.MaxTextWidth" check detects if any 
            // single line is too long to fit within the text area, this can only happen if there is a 
            // long span of text with no spaces.

            Debug.Print($"IsTextTrimmed: { formattedText.MaxTextWidth}, {formattedText.Width}, {(formattedText.MaxTextWidth > formattedText.Width)}");
            return (formattedText.MaxTextWidth > formattedText.Width) || (formattedText.MaxTextHeight > formattedText.Height);
        }

        public static bool IsTextTrimmed(TextBlock textBlock)
        {
            if (textBlock.TextTrimming == TextTrimming.None)
                return false;

            var roundingFlag = false;
            if (textBlock.UseLayoutRounding)
            {
                textBlock.UseLayoutRounding = false;
                // Delay after UseLayoutRounding was change
                textBlock.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                roundingFlag = true;
            }

            var delta = textBlock.UseLayoutRounding ? -0.00001d : 0.0d;
            if (textBlock.TextWrapping == TextWrapping.NoWrap)
            {
                /*if (textBlock.UseLayoutRounding)
                {
                    textBlock.UseLayoutRounding = false;
                    // Delay after UseLayoutRounding was change
                    textBlock.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                }*/
                /*if (!textBlock.IsMeasureValid)
                {
                    textBlock.UpdateLayout();
                    textBlock.InvalidateArrange();
                    textBlock.InvalidateMeasure();
                    // Delay after UseLayoutRounding was change
                    textBlock.Dispatcher.Invoke(() =>
                    {
                        textBlock.InvalidateArrange();
                        textBlock.InvalidateMeasure();
                        textBlock.UpdateLayout();
                    }, DispatcherPriority.Render);
                }*/
                textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var desiredWidth = textBlock.DesiredSize.Width;
                var actualWidth = textBlock.ActualWidth + textBlock.Margin.Left + textBlock.Margin.Right;// + delta;
                Debug.Print($"IsTextTrimmed: { desiredWidth}, {actualWidth}, {desiredWidth > actualWidth}");
                if (roundingFlag)
                {
                    textBlock.UseLayoutRounding = true; // restore UseLayoutRounding property
                    // Delay after UseLayoutRounding was change
                    textBlock.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                }

                return desiredWidth > actualWidth;
            }
            else
            {
                textBlock.Measure(new Size(
                    textBlock.ActualWidth + textBlock.Margin.Left + textBlock.Margin.Right,// + 0.0001d,
                    double.PositiveInfinity));
                var desiredHeight1 = textBlock.DesiredSize.Height;
                var actualHeight1 = textBlock.ActualHeight + textBlock.Margin.Top + textBlock.Margin.Bottom;// + delta;
                Debug.Print($"IsTextTrimmed: { desiredHeight1}, {actualHeight1}, {desiredHeight1 > actualHeight1}");
                if (roundingFlag)
                {
                    textBlock.UseLayoutRounding = true; // restore UseLayoutRounding property
                    // Delay after UseLayoutRounding was change
                    textBlock.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                }
                return desiredHeight1 > actualHeight1;
            }
        }

        public static bool IsTextTrimmed2(TextBlock textBlock)
        {
            // see also https://stackoverflow.com/questions/1041820/how-can-i-determine-if-my-textblock-text-is-being-trimmed
            if (textBlock.TextWrapping != TextWrapping.NoWrap) return false;

            textBlock.Measure(new Size(double.PositiveInfinity, height: double.PositiveInfinity));
            return (textBlock.ActualWidth + textBlock.Margin.Left + textBlock.Margin.Right) < textBlock.DesiredSize.Width ||
                   (textBlock.ActualHeight + textBlock.Margin.Top + textBlock.Margin.Bottom) < textBlock.DesiredSize.Height;
        }


    }
}
