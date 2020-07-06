using System.Windows;
using System.Windows.Controls;

namespace IconViewer
{
    /// <summary>
    /// Interaction logic for MyFunkyControl.xaml
    /// </summary>
    public partial class MyFunkyControl
    {
        public MyFunkyControl()
        {
            InitializeComponent();
            Template = Resources["BBB"] as ControlTemplate;
        }

        public static readonly DependencyProperty HeadingProperty =
            DependencyProperty.Register("Heading", typeof(string),
                typeof(MyFunkyControl), new PropertyMetadata(HeadingChanged));

        private static void HeadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MyFunkyControl)d).Heading = e.NewValue as string;
        }

        public string Heading { get; set; }

    }
}
