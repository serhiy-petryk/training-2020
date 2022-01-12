using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ItemsControlDragDrop.Code
{
    public class DropTargetInsertionAdorner : DropTargetAdorner
    {
        public DropTargetInsertionAdorner(UIElement adornedElement)
          : base(adornedElement)
        {
            // Panel.SetZIndex(this, 999999);
        }

        private static int cnt;
        protected override void OnRender(DrawingContext drawingContext)
        {
            var panel = (Panel)AdornedElement;
            var dropInfo = DragDropHelper._dropInfo;

            var insertIndex = -1;
            Debug.Print($"Render: {cnt++}");
            for (var i = 0; i < panel.Children.Count; i++)
            {
                var item = panel.Children[i];
                var p = dropInfo._currentEventArgs.GetPosition(item);
                var bounds = VisualTreeHelper.GetDescendantBounds(item);
                if (bounds.Contains(p))
                {
                    insertIndex = i + (p.Y < bounds.Top + bounds.Height / 2 ? 0 : 1);
                    break;
                }
            }

            if (insertIndex < 0)
                insertIndex = dropInfo._currentEventArgs.GetPosition(panel).Y < panel.Height / 2
                    ? 0
                    : panel.Children.Count + 1;

            double insertPosition;
            if (insertIndex == 0)
            {
                var itemRect = new Rect(panel.Children[0].TranslatePoint(new Point(), panel), panel.Children[0].RenderSize);
                insertPosition = itemRect.Top;
            }
            else if (insertIndex < panel.Children.Count)
            {
                var itemRect1 = new Rect(panel.Children[insertIndex - 1].TranslatePoint(new Point(), panel), panel.Children[insertIndex - 1].RenderSize);
                var itemRect2 = new Rect(panel.Children[insertIndex].TranslatePoint(new Point(), panel), panel.Children[insertIndex].RenderSize);
                insertPosition = (itemRect1.Top + itemRect1.Height + itemRect2.Top) / 2;
            }
            else
            {
                var itemRect = new Rect(panel.Children[panel.Children.Count - 1].TranslatePoint(new Point(), panel), panel.Children[0].RenderSize);
                insertPosition = itemRect.Top + itemRect.Height;
            }

            var p1 = new Point(0, insertPosition);
            var p2 = new Point(panel.ActualWidth, insertPosition);
            var rotation1 = 0.0;
            drawingContext.DrawLine(m_Pen, p1, p2);
            this.DrawTriangle(drawingContext, p1, rotation1);
            this.DrawTriangle(drawingContext, p2, 180 + rotation1);

            return;
            // Debug.Print($"Mouse: {p}");

            //var aa1 = panel.GetVisualParents();
            //var p = Mouse.GetPosition(panel);
            //var p1 = Mouse.GetPosition(null);
            // var p1 = MouseDevice.GetPosition((IInputElement)panel);
            // panel.Children
            //            Debug.Print($"Mouse: {p}, {p1}");

            var itemsControl = AdornedElement.GetVisualParents().OfType<ItemsControl>().FirstOrDefault();
            //var itemsControl = this.DropInfo.VisualTarget as ItemsControl;
 
             if (itemsControl != null)
             {
                 // Get the position of the item at the insertion index. If the insertion point is
                 // to be after the last item, then get the position of the last item and add an 
                 // offset later to draw it at the end of the list.
                 ItemsControl itemParent;



 
                 /*if (this.DropInfo.VisualTargetItem != null)
                 {
                     itemParent = ItemsControl.ItemsControlFromItemContainer(this.DropInfo.VisualTargetItem);
                 }
                 else*/
            {
                itemParent = itemsControl;
                 }

                 // var index = Math.Min(this.DropInfo.InsertIndex, itemParent.Items.Count - 1);
                 var index = Math.Min(3, itemParent.Items.Count - 1);
                 var itemContainer = (UIElement)itemParent.ItemContainerGenerator.ContainerFromIndex(index);
 
                 if (itemContainer != null)
                 {
                     var itemRect = new Rect(itemContainer.TranslatePoint(new Point(), this.AdornedElement), itemContainer.RenderSize);
                     Point point1, point2;
                     double rotation = 0;

//                     if (this.DropInfo.VisualTargetOrientation == Orientation.Vertical)
                         if (true)
                     {
                            //if (this.DropInfo.InsertIndex == itemParent.Items.Count)
                         {
                             //itemRect.Y += itemContainer.RenderSize.Height;
                         }
 
                         point1 = new Point(itemRect.X, itemRect.Y);
                         point2 = new Point(itemRect.Right, itemRect.Y);
                     }
                     else
                     {
                         /*var itemRectX = itemRect.X;
 
                         if (this.DropInfo.VisualTargetFlowDirection == FlowDirection.LeftToRight && this.DropInfo.InsertIndex == itemParent.Items.Count)
                         {
                             itemRectX += itemContainer.RenderSize.Width;
                         }
                         else if (this.DropInfo.VisualTargetFlowDirection == FlowDirection.RightToLeft && this.DropInfo.InsertIndex != itemParent.Items.Count)
                         {
                             itemRectX += itemContainer.RenderSize.Width;
                         }
 
                         point1 = new Point(itemRectX, itemRect.Y);
                         point2 = new Point(itemRectX, itemRect.Bottom);
                         rotation = 90;*/
                     }
 
                     drawingContext.DrawLine(m_Pen, point1, point2);
                     this.DrawTriangle(drawingContext, point1, rotation);
                     this.DrawTriangle(drawingContext, point2, 180 + rotation);
                 }
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
            // Panel.SetZIndex(m_Triangle, 999999);
            m_Triangle.Freeze();
        }

        private static readonly Pen m_Pen;
        private static readonly PathGeometry m_Triangle;
    }
}