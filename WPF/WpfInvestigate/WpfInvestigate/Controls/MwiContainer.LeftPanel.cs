using System.Windows;

namespace WpfInvestigate.Controls
{
    public partial class MwiContainer
    {
        public static readonly DependencyProperty LeftPanelProperty = DependencyProperty.Register("LeftPanel", typeof(FrameworkElement), typeof(MwiContainer), new FrameworkPropertyMetadata(null, LeftPanel_OnPropertyChanged));

        public FrameworkElement LeftPanel
        {
            get => (FrameworkElement)GetValue(LeftPanelProperty);
            set => SetValue(LeftPanelProperty, value);
        }
        private static void LeftPanel_OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
