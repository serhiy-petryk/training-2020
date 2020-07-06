using System.Windows;
using System.Windows.Media.Animation;

namespace WpfInvestigate.Obsolete
{
    public class BeforeAnimateEventArgs : RoutedEventArgs
    { // Uses in PathToggleButton
        public bool IsFirstRun { get; }
        public Storyboard[] Storyboards { get; }

        public BeforeAnimateEventArgs(RoutedEvent routedEvent, Storyboard[] storyboards, bool isFirstRun) : base(routedEvent)
        {
            Storyboards = storyboards;
            IsFirstRun = isFirstRun;
        }
    }
}
