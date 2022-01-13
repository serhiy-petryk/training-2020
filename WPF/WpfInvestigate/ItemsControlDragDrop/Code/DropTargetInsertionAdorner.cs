﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ItemsControlDragDrop.Code
{
    public class DropTargetInsertionAdorner : DropTargetAdorner
    {
        public DropTargetInsertionAdorner(UIElement adornedElement) : base(adornedElement)
        {
        }

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

            var insertIndex = DragDropHelper.GetOffsetIndex(control);
            double insertPosition;
            if (insertIndex == 0)
            {
                insertPosition = getX(panel.Children[0].TranslatePoint(new Point(), control));
            }
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

            if (orientation == Orientation.Vertical)
            {
                var p1 = new Point(panel.Margin.Left, insertPosition);
                var p2 = new Point(panel.Margin.Left + panel.ActualWidth - panel.Margin.Right, insertPosition);
                drawingContext.DrawLine(m_Pen, p1, p2);
                DrawTriangle(drawingContext, p1, 0);
                DrawTriangle(drawingContext, p2, 180);
            }
            else
            {
                // Orientation horizontal 
                var p1 = new Point(insertPosition, panel.Margin.Top);
                var p2 = new Point(insertPosition, panel.Margin.Top + panel.ActualHeight - panel.Margin.Bottom);
                drawingContext.DrawLine(m_Pen, p1, p2);
                DrawTriangle(drawingContext, p1, 90);
                DrawTriangle(drawingContext, p2, 270);
            }
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

            m_Pen = new Pen(Brushes.Gray, 2);
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