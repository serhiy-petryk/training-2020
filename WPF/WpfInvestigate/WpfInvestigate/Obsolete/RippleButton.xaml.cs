// From https://github.com/Taka414/RippleEffectControl

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace WpfInvestigate.Obsolete
{
    /// <summary>
    /// Interaction logic for RippleButton.xaml
    /// </summary>
    public partial class RippleButton
    {
        public static readonly DependencyProperty RippleColorProperty = DependencyProperty.Register("RippleColor", typeof(Brush), typeof(RippleButton), new PropertyMetadata(Brushes.White));
        public Brush RippleColor
        {
            get { return (Brush)GetValue(RippleColorProperty); }
            set { SetValue(RippleColorProperty, value); }
        }

        public RippleButton()
        {
            InitializeComponent();
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            var mousePos = e.GetPosition(this);
            // var ellipse = GetTemplateChild("CircleEffect") as Ellipse;
            DoAnimation(mousePos);
        }

        private void DoAnimation(Point mousePos)
        {
            // クリック位置からRippleの中心を取る
            // Point mousePos = (e as MouseButtonEventArgs).GetPosition(this);

            var ellipse = GetTemplateChild("CircleEffect") as Ellipse;
            ellipse.Margin = new Thickness(mousePos.X, mousePos.Y, 0, 0);

            // アニメーションの動作の指定
            var storyboard = (FindResource("RippleAnimation") as Storyboard).Clone();

            // 円の最大の大きさ -> コントロールの大きさの倍
            var newSize = Math.Max(ActualWidth, ActualHeight) * 3;
            var oldMargin = new Thickness(mousePos.X, mousePos.Y, 0, 0);
            var newMargin = new Thickness(mousePos.X - newSize / 2, mousePos.Y - newSize / 2, 0, 0);

            (storyboard.Children[2] as ThicknessAnimation).From = oldMargin;
            (storyboard.Children[2] as ThicknessAnimation).To = newMargin;
            (storyboard.Children[3] as DoubleAnimation).To = newSize;

            ellipse.BeginStoryboard(storyboard);
        }

    }
}
