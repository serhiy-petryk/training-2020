using System.Windows;
using System.Windows.Controls;

namespace Movable.Controls
{
    public partial class SampleResizingContent : UserControl
    {
        private static int Unique = 1;
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title",
            typeof(string), typeof(SampleResizingContent), new PropertyMetadata((Unique++).ToString()));
        //========================

        public SampleResizingContent()
        {
            InitializeComponent();
            DataContext = this;
        }

        /*protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            Panel.SetZIndex(this, Unique++);
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (Focusable)
                Focus();
        }*/

    }
}
