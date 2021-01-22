using System.Windows;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Interaction logic for MwiChild1.xaml
    /// </summary>
    public partial class MwiChild1
    {
        public MwiChild1()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string Title { get; set; } = "Title";

        public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content),
            typeof(object), typeof(MwiChild1), new UIPropertyMetadata(null));
        public new object Content
        {
            get => (object)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

    }
}
