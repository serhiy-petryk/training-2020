using System.Windows;
using System.Windows.Controls;

namespace WpfInvestigate.Controls
{
    public class DragAdornerControl: ItemsControl // , INotifyPropertyChanged
    {
        public static readonly DependencyProperty DragDropEffectProperty = DependencyProperty.Register("DragDropEffect",
            typeof(DragDropEffects), typeof(DragAdornerControl), new FrameworkPropertyMetadata(DragDropEffects.Move));
        public DragDropEffects DragDropEffect
        {
            get => (DragDropEffects)GetValue(DragDropEffectProperty);
            set => SetValue(DragDropEffectProperty, value);
        }

        /*public DragDropEffects DragDropEffects { get; set; }= DragDropEffects.Move;

        #region ===========  INotifyPropertyChanged  ===============
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion*/
    }
}
