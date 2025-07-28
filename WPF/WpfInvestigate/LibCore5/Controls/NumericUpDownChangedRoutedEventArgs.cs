using System.Windows;

namespace LibCore5.Controls
{
    public class NumericUpDownChangedRoutedEventArgs : RoutedEventArgs
    {
        public double Interval { get; set; }

        public NumericUpDownChangedRoutedEventArgs(RoutedEvent routedEvent, double interval) : base(routedEvent)
        {
            Interval = interval;
        }
    }
}