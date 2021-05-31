﻿// Taken from https://wpf.2000things.com/tag/isenabled/ - it's readonly text box => allow to copy text in UI
using System.Windows;
using System.Windows.Controls;

namespace WpfInvestigate.Controls
{
    public class LabelBox: TextBox
    {
        static LabelBox()
        {
            IsEnabledProperty.OverrideMetadata(typeof(LabelBox), new UIPropertyMetadata(true, IsEnabledPropertyChanged, CoerceIsEnabled));
        }

        private static void IsEnabledPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            // Overriding PropertyChanged results in merged metadata, which is what we want--
            // the PropertyChanged logic in UIElement.IsEnabled will still get invoked.
        }

        private static object CoerceIsEnabled(DependencyObject source, object value)
        {
            return value;
        }

        public LabelBox()
        {
            IsReadOnly = true;
        }
    }
}
