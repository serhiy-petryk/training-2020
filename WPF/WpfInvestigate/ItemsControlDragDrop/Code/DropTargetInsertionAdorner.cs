using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ItemsControlDragDrop.Code
{
    public class DropTargetInsertionAdorner : DropTargetAdorner
    {
        public DropTargetInsertionAdorner(UIElement adornedElement) : base(adornedElement) {}

        protected override void OnRender(DrawingContext drawingContext)
        {
            var control = (ItemsControl)AdornedElement;
            var panel = DragDropHelper.GetItemsHost(control);

            var orientation = DragDropHelper.GetItemsPanelOrientation(control);
            Func<Point, double> getX;
            Func<Size, double> getWidth;
            if (orientation == Orientation.Horizontal)
            {
                getX = point => point.X;
                getWidth = size => size.Width;
            }
            else
            {
                getX = point => point.Y;
                getWidth = size => size.Height;
            }

            var insertIndex = DragDropHelper.GetInsertIndex(control, out int firstItemOffset);
            double insertPosition;
            if (insertIndex == 0)
                insertPosition = getX(panel.Children[0].TranslatePoint(new Point(), control));
            else if (insertIndex < panel.Children.Count)
            {
                var p1 = panel.Children[insertIndex - 1].TranslatePoint(new Point(), control);
                var p2 = panel.Children[insertIndex].TranslatePoint(new Point(), control);
                insertPosition = (getX(p1) + getWidth(panel.Children[insertIndex - 1].RenderSize) + getX(p2)) / 2;
            }
            else
            {
                var p1 = panel.Children[panel.Children.Count - 1].TranslatePoint(new Point(), control);
                insertPosition = getX(p1) + getWidth(panel.Children[panel.Children.Count - 1].RenderSize);
            }

            var p = panel.TranslatePoint(new Point(), control);
            var pp1 = orientation == Orientation.Vertical ? new Point(p.X, insertPosition): new Point(insertPosition, p.Y);
            var pp2 = orientation == Orientation.Vertical ? new Point(p.X + panel.ActualWidth, insertPosition): new Point(insertPosition, p.Y + panel.ActualHeight);
            drawingContext.DrawLine(m_Pen, pp1, pp2);
            var rotation = orientation == Orientation.Vertical ? 0.0 : 90.0;
            DrawTriangle(drawingContext, pp1, rotation);
            DrawTriangle(drawingContext, pp2, rotation + 180.0);
        }

        private void DrawTriangle(DrawingContext drawingContext, Point origin, double rotation)
        {
            drawingContext.PushTransform(new TranslateTransform(origin.X, origin.Y));
            drawingContext.PushTransform(new RotateTransform(rotation));

            drawingContext.DrawGeometry(m_Pen.Brush, null, m_Triangle);

            drawingContext.Pop();
            drawingContext.Pop();
        }

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
    }
}