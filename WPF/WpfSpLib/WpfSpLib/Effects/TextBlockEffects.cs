using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfSpLib.Effects
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

        private static bool IsTextTrimmed(TextBlock textBlock)
        {
            // From https://stackoverflow.com/questions/1041820/how-can-i-determine-if-my-textblock-text-is-being-trimmed
            var typeface = new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch);
            var formattedText =
                new FormattedText(textBlock.Text, System.Threading.Thread.CurrentThread.CurrentCulture,
                        textBlock.FlowDirection, typeface, textBlock.FontSize, textBlock.Foreground)
                    {MaxTextWidth = textBlock.ActualWidth};

            // When the maximum text width of the FormattedText instance is set to the actual
            // width of the textBlock, if the textBlock is being trimmed to fit then the formatted
            // text will report a larger height than the textBlock. Should work whether the
            // textBlock is single or multi-line.
            // The "formattedText.MinWidth > formattedText.MaxTextWidth" check detects if any 
            // single line is too long to fit within the text area, this can only happen if there is a 
            // long span of text with no spaces.
            return (formattedText.Height > textBlock.ActualHeight || formattedText.MinWidth > formattedText.MaxTextWidth);
        }

    }
}
