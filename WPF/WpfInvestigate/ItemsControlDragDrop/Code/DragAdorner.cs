using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ItemsControlDragDrop.Code
{
    internal class DragAdorner : Adorner
    {
        public DragAdorner(UIElement adornedElement, object data) : base(adornedElement)
        {
            var sourceData = data is IEnumerable ? ((IEnumerable) data).OfType<object>().ToArray() : new[] {data};
            if (sourceData.Length > 5)
            {
                var tempData = sourceData.Take(4).ToList();
                tempData.Add("more items ...");
                sourceData = tempData.ToArray();
            }
            var template = Application.Current.Resources["DragAdorner"] as DataTemplate;
            var itemsControl = new ItemsControl { ItemsSource = sourceData, ItemTemplate = template };
            var border = new Border { Child = itemsControl };
            m_Adornment = border;

            m_AdornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            m_AdornerLayer.Add(this);
            IsHitTestVisible = false;
        }

        public void Detach()
        {
            m_AdornerLayer.Remove(this);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var adornerPos = DragDropHelper._dropInfo._currentEventArgs.GetPosition(AdornedElement);
            m_Adornment.RenderTransform = new TranslateTransform(adornerPos.X + 4.0, -m_Adornment.ActualHeight + adornerPos.Y - 1.0);
            m_AdornerLayer.Update(AdornedElement);
        }

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
        private readonly FrameworkElement m_Adornment;
    }
}