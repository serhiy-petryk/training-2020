using System.Windows;
using System.Windows.Controls;

namespace IconViewer
{
    public class MyFunkyControl2: ContentControl
    {
        public MyFunkyControl2()
        {
            Template = Application.Current.Resources["BBB2"] as ControlTemplate;
        }

        public static readonly DependencyProperty HeadingProperty =
            DependencyProperty.Register("Heading", typeof(string),
                typeof(MyFunkyControl2), new PropertyMetadata(HeadingChanged));

        private static void HeadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MyFunkyControl2)d).Heading = e.NewValue as string;
        }

        public string Heading { get; set; }

    }
}
