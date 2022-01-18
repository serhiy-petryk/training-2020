using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using WpfInvestigate.Controls;

namespace WpfInvestigate.Helpers
{
    internal class DragAdorner : Adorner
    {
        public DragAdorner(UIElement adornedElement, object dragDropData) : base(adornedElement)
        {
            var sourceData = (dragDropData is IEnumerable enumerable ? enumerable.OfType<object>().ToArray() : new[] {dragDropData})
                .Select(a => GetDragItemLabel(a)).ToArray();

            if (sourceData.Length > 5)
            {
                var tempData = sourceData.Take(4).ToList();
                tempData.Add("more items ...");
                sourceData = tempData.ToArray();
            }
            ((ItemsControl)m_Adornment.Child).ItemsSource = sourceData;
            // m_Adornment.ItemsSource = sourceData;

            m_AdornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            m_AdornerLayer.Add(this);
            IsHitTestVisible = false;
        }

        public void UpdateUI(DragEventArgs e, ItemsControl itemsControl)
        {
            var adornerPos = e.GetPosition(AdornedElement);
            var transforms = new TransformGroup();
            var actualTransform = itemsControl.GetActualLayoutTransforms();
            transforms.Children.Add(actualTransform);
            transforms.Children.Add(new TranslateTransform(adornerPos.X + 4.0 * actualTransform.Value.M11,
                adornerPos.Y - (m_Adornment.ActualHeight + 1.0) * actualTransform.Value.M22));
            m_Adornment.RenderTransform = transforms;
            m_AdornerLayer.Update(AdornedElement);
        }

        public void Detach() => m_AdornerLayer.Remove(this);

        protected override Size ArrangeOverride(Size finalSize)
        {
            m_Adornment.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            m_Adornment.Measure(constraint);
            return m_Adornment.DesiredSize;
        }

        protected override Visual GetVisualChild(int index) => m_Adornment;
        protected override int VisualChildrenCount => 1;

        private readonly AdornerLayer m_AdornerLayer;

        #region ===========  Static section  =============
        static DragAdorner()
        {
            var template = Application.Current.Resources["DragAdorner"] as DataTemplate;
            var itemsControl = new ItemsControl { ItemTemplate = template };
            m_Adornment = new Border { Child = itemsControl };
            // m_Adornment = new DragAdornerControl();
        }

        private static object GetDragItemLabel(object item)
        {
            if (item is TabItem tabItem)
                return tabItem.Header;
            return item;
        }


        private static readonly Border m_Adornment;
        // private static readonly ItemsControl m_Adornment;
        #endregion
    }
}