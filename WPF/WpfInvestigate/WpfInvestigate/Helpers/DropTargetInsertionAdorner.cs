using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace WpfInvestigate.Helpers
{
    public class DropTargetInsertionAdorner : Adorner
    {
        public DropTargetInsertionAdorner(UIElement adornedElement) : base(adornedElement)
        {
            m_AdornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            m_AdornerLayer.Add(this);
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (!DragDropHelper._dragInfo.InsertIndex.HasValue)
                return;

            var control = (ItemsControl)AdornedElement;
            var panel = DragDropHelper.GetItemsHost(control);
            var orientation = DragDropHelper.GetItemsPanelOrientation(control);
            var relativeItem = panel.Children.Count == 0 ? panel
                : (DragDropHelper._dragInfo.InsertIndex.Value < panel.Children.Count
                    ? panel.Children[DragDropHelper._dragInfo.InsertIndex.Value]
                    : panel.Children[panel.Children.Count - 1]) as FrameworkElement;
            var r = relativeItem.GetVisibleRect(control);

            double insertPosition;
            if (DragDropHelper._dragInfo.InsertIndex.Value < panel.Children.Count)
                insertPosition = orientation == Orientation.Vertical ? r.Top : r.Left;
            else
                insertPosition = orientation == Orientation.Vertical ? r.Bottom : r.Right;
            var p1 = orientation == Orientation.Vertical ? new Point(r.X, insertPosition) : new Point(insertPosition, r.Y);
            var p2 = orientation == Orientation.Vertical ? new Point(r.X + r.Width, insertPosition) : new Point(insertPosition, r.Y + r.Height);
            var rotation = orientation == Orientation.Vertical ? 0.0 : 90.0;

            drawingContext.DrawLine(m_Pen, p1, p2);
            DrawTriangle(drawingContext, p1, rotation);
            DrawTriangle(drawingContext, p2, rotation + 180.0);
        }

        private void DrawTriangle(DrawingContext drawingContext, Point origin, double rotation)
        {
            drawingContext.PushTransform(new TranslateTransform(origin.X, origin.Y));
            drawingContext.PushTransform(new RotateTransform(rotation));

            drawingContext.DrawGeometry(m_Pen.Brush, null, m_Triangle);

            drawingContext.Pop();
            drawingContext.Pop();
        }

        public void Detach() => m_AdornerLayer.Remove(this);

        private readonly AdornerLayer m_AdornerLayer;

        #region ===========  Static section  =============
        static DropTargetInsertionAdorner()
        {
            // Create the pen and triangle in a static constructor and freeze them to improve performance.
            const int triangleSize = 5;

            m_Pen = new Pen(Brushes.Magenta, 2);
            m_Pen.Freeze();

            var firstLine = new LineSegment(new Point(0, -triangleSize), false);
            firstLine.Freeze();
            var secondLine = new LineSegment(new Point(0, triangleSize), false);
            secondLine.Freeze();

            var figure = new PathFigure { StartPoint = new Point(triangleSize, 0) };
            figure.Segments.Add(firstLine);
            figure.Segments.Add(secondLine);
            figure.Freeze();

            m_Triangle = new PathGeometry();
            m_Triangle.Figures.Add(figure);
            m_Triangle.Freeze();
        }

        private static readonly Pen m_Pen;
        private static readonly PathGeometry m_Triangle;
        #endregion
    }
}