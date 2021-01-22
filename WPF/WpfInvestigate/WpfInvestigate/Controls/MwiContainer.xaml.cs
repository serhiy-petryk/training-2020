using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Interaction logic for MwiContainer.xaml
    /// </summary>
    [ContentProperty("Children")]
    public partial class MwiContainer
    {
        public ObservableCollection<FrameworkElement> Children { get; set; } = new ObservableCollection<FrameworkElement>();
        public MwiContainer()
        {
            InitializeComponent();
            Children.CollectionChanged += Children_OnCollectionChanged;

        }

        private void Children_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var mwiChild = Children[e.NewStartingIndex];
                    MwiPanel.Children.Add(mwiChild);
                    /*SetMwiContainer(mwiChild, this);

                    if (ActiveMwiChild?.WindowState == WindowState.Maximized && mwiChild.Resizable)
                        mwiChild.Loaded += (s, a) => mwiChild.WindowState = WindowState.Maximized;

                    if (mwiChild.Position.X < 0 || mwiChild.Position.Y < 0)
                        mwiChild.Position = new Point(_windowOffset, _windowOffset);
                    _windowOffset += WINDOW_OFFSET_STEP;
                    if ((_windowOffset + mwiChild.Width > ActualWidth) || (_windowOffset + mwiChild.Height > ActualHeight))
                        _windowOffset = 0;

                    MwiCanvas.Children.Add(mwiChild);
                    ActiveMwiChild = mwiChild;*/
                    break;

                case NotifyCollectionChangedAction.Remove:
                    var oldChild = (MwiChild)e.OldItems[0];
                    /*var oldActiveMwiChild = ActiveMwiChild == oldChild ? null : ActiveMwiChild;
                    ActiveMwiChild = null; // must be null because sometimes there is an error on WindowTabs.Remove (select window tab => press delete button on active MwiChild): Index was outside the bounds of the array

                    MwiCanvas.Children.Remove(oldChild);
                    ActiveMwiChild = oldActiveMwiChild ?? GetTopChild();*/
                    break;

                case NotifyCollectionChangedAction.Reset:
                    /*while (Children.Count > 0)
                        Children[0].Close();*/
                    break;
            }
        }
    }
}
