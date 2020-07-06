﻿using System.Windows;
using System.Windows.Controls;
using MyWpfMwi.Common;

namespace MyWpfMwi.Controls
{
    // Based on from https://blog.scottlogic.com/2011/01/31/automatically-showing-tooltips-on-a-trimmed-textblock-silverlight-wpf.html
    // usage:
    // 1. controls:TextBlockExt.AutoTooltipTarget:
    // <Thumb x:Name="MoveThumb" Height="30" VerticalAlignment="Top" Opacity="0" controls:TextBlockExt.AutoTooltipTarget="{Binding ElementName=Header}" ...
    // <Grid IsHitTestVisible="False">
    //      <TextBlock x:Name="Header" Grid.Column="1" TextTrimming="CharacterEllipsis" TextWrapping="NoWrap" Text="{TemplateBinding Title}"/>
    // (original but not here) 2. controls:TextBlockExt.AutoTooltip:
    // <TextBlock Text="The rain in Spain stays mainly in the plain" util:TextBlockUtils.AutoTooltip="True"/>

    public class TextBlockExt
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
        public static readonly DependencyProperty AutoTooltipTargetProperty = DependencyProperty.RegisterAttached("AutoTooltipTarget", typeof(TextBlock), typeof(TextBlockExt), new PropertyMetadata(null, OnAutoTooltipTargetPropertyChanged));

        private static void OnAutoTooltipTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = e.NewValue as TextBlock;
            if (textBlock == null)
                return;

            textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
            ComputeAutoTooltip(textBlock, d);
            textBlock.SizeChanged += (sender, eSizeChanged) => ComputeAutoTooltip(textBlock, d);
        }

        /// <summary>
        /// Assigns the ToolTip for the given TextBlock based on whether the text is trimmed
        /// </summary>
        private static void ComputeAutoTooltip(TextBlock textBlock, DependencyObject toolTipPlaceHolder = null)
        {
            var isTextTrimmed = Tips.IsTextTrimmed(textBlock);
            ToolTipService.SetToolTip(toolTipPlaceHolder ?? textBlock, isTextTrimmed ? textBlock.Text : null);
        }
    }
}
