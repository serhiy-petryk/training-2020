using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace ItemsControlDragDrop.Code
{
    // From https://www.wundervisionenvisionthefuture.com/blog/wpf-c-drag-and-drop-icon-adorner
    //Adorner subclass specific to this control
    public class DraggableAdorner : Adorner
    {
        Rect renderRect;
        Brush renderBrush;
        public Point CenterOffset;
        public DraggableAdorner(FrameworkElement adornedElement) : base(adornedElement)
        {
            renderRect = new Rect(adornedElement.RenderSize);
            this.IsHitTestVisible = false;
            //Clone so that it can be modified with on modifying the original
            // renderBrush = adornedElement.Background.Clone();
            renderBrush = Brushes.Red;
            CenterOffset = new Point(-renderRect.Width / 2, -renderRect.Height / 2);
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(renderBrush, null, renderRect);
        }
    }
}
