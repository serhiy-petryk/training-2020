using System.Windows;

namespace WpfSpLib.ViewModels
{
    public class MwiAppViewModel: DependencyObject
    {
        #region ================  Static section  =====================

        public static MwiAppViewModel Instance = new MwiAppViewModel();
        #endregion

        #region ================  Instance section  ====================
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register(nameof(ScaleValue), typeof(double), typeof(MwiAppViewModel), new UIPropertyMetadata(1.0));
        public double ScaleValue
        {
            get => (double)GetValue(ScaleValueProperty);
            set => SetValue(ScaleValueProperty, value);
        }
        #endregion
    }
}
