// Source: https://wpfspark.wordpress.com/2011/06/08/clipborder-a-wpf-border-that-clips/
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfInvestigate.Controls
{
    /// <summary>
    /// Border which allows Clipping to its border.
    /// Useful especially when you need to clip to round corners.
    /// </summary>
    public class ClipBorder : Border
    {
        protected override void OnRender(DrawingContext dc)
        {
            OnApplyChildClip();
            base.OnRender(dc);
        }

        public override UIElement Child
        {
            get => base.Child;
            set
            {
                if (Child != value)
                {
                    if (Child != null) // Restore original clipping of the old child
                        Child.SetValue(ClipProperty, _oldClip);

                    if (value != null) // Store the current clipping of the new child
                        _oldClip = value.ReadLocalValue(UIElement.ClipProperty);
                    else // If we dont set it to null we could leak a Geometry object
                        _oldClip = null;

                    base.Child = value;
                }
            }
        }

        protected virtual void OnApplyChildClip()
        {
            var child = Child;
            if (child != null)
            {
                // Get the geometry of a rounded rectangle border based on the BorderThickness and CornerRadius
                _clipGeometry = GetRoundRectangle(new Rect(Child.RenderSize), this.BorderThickness, this.CornerRadius);
                child.Clip = _clipGeometry;
            }
        }

        private Geometry _clipGeometry = null;
        private object _oldClip;

        private const double Tolerance = 0.0001;
        private static Geometry GetRoundRectangle(Rect baseRect, Thickness thickness, CornerRadius cornerRadius)
        {
            // Normalizing the corner radius
            if (cornerRadius.TopLeft < Tolerance) cornerRadius.TopLeft = 0.0;
            if (cornerRadius.TopRight < Tolerance) cornerRadius.TopRight = 0.0;
            if (cornerRadius.BottomLeft < Tolerance) cornerRadius.BottomLeft = 0.0;
            if (cornerRadius.BottomRight < Tolerance) cornerRadius.BottomRight = 0.0;

            // Taking the border thickness into account
            var leftHalf = thickness.Left * 0.5;
            if (leftHalf < Tolerance) leftHalf = 0.0;
            var topHalf = thickness.Top * 0.5;
            if (topHalf < Tolerance) topHalf = 0.0;
            var rightHalf = thickness.Right * 0.5;
            if (rightHalf < Tolerance) rightHalf = 0.0;
            var bottomHalf = thickness.Bottom * 0.5;
            if (bottomHalf < Tolerance) bottomHalf = 0.0;

            // Create the rectangles for the corners that needs to be curved in the base rectangle 
            // TopLeft Rectangle 
            var topLeftRect = new Rect(baseRect.Location.X, 
                                        baseRect.Location.Y,
                                        Math.Max(0.0, cornerRadius.TopLeft - leftHalf),
                                        Math.Max(0.0, cornerRadius.TopLeft - rightHalf));
            // TopRight Rectangle 
            var topRightRect = new Rect(baseRect.Location.X + baseRect.Width - cornerRadius.TopRight + rightHalf,
                                         baseRect.Location.Y,
                                         Math.Max(0.0, cornerRadius.TopRight - rightHalf),
                                         Math.Max(0.0, cornerRadius.TopRight - topHalf));
            // BottomRight Rectangle
            var bottomRightRect = new Rect(baseRect.Location.X + baseRect.Width - cornerRadius.BottomRight + rightHalf,
                                            baseRect.Location.Y + baseRect.Height - cornerRadius.BottomRight + bottomHalf,
                                            Math.Max(0.0, cornerRadius.BottomRight - rightHalf),
                                            Math.Max(0.0, cornerRadius.BottomRight - bottomHalf));
            // BottomLeft Rectangle 
            var bottomLeftRect = new Rect(baseRect.Location.X,
                                           baseRect.Location.Y + baseRect.Height - cornerRadius.BottomLeft + bottomHalf,
                                           Math.Max(0.0, cornerRadius.BottomLeft - leftHalf),
                                           Math.Max(0.0, cornerRadius.BottomLeft - bottomHalf));

            // Adjust the width of the TopLeft and TopRight rectangles so that they are proportional to the width of the baseRect 
            if (topLeftRect.Right > topRightRect.Left)
            {
                var newWidth = (topLeftRect.Width / (topLeftRect.Width + topRightRect.Width)) * baseRect.Width;
                topLeftRect = new Rect(topLeftRect.Location.X, topLeftRect.Location.Y, newWidth, topLeftRect.Height);
                topRightRect = new Rect(baseRect.Left + newWidth, topRightRect.Location.Y, Math.Max(0.0, baseRect.Width - newWidth), topRightRect.Height);
            }

            // Adjust the height of the TopRight and BottomRight rectangles so that they are proportional to the height of the baseRect
            if (topRightRect.Bottom > bottomRightRect.Top)
            {
                var newHeight = (topRightRect.Height / (topRightRect.Height + bottomRightRect.Height)) * baseRect.Height;
                topRightRect = new Rect(topRightRect.Location.X, topRightRect.Location.Y, topRightRect.Width, newHeight);
                bottomRightRect = new Rect(bottomRightRect.Location.X, baseRect.Top + newHeight, bottomRightRect.Width, Math.Max(0.0, baseRect.Height - newHeight));
            }

            // Adjust the width of the BottomLeft and BottomRight rectangles so that they are proportional to the width of the baseRect
            if (bottomRightRect.Left < bottomLeftRect.Right)
            {
                var newWidth = (bottomLeftRect.Width / (bottomLeftRect.Width + bottomRightRect.Width)) * baseRect.Width;
                bottomLeftRect = new Rect(bottomLeftRect.Location.X, bottomLeftRect.Location.Y, newWidth, bottomLeftRect.Height);
                bottomRightRect = new Rect(baseRect.Left + newWidth, bottomRightRect.Location.Y, Math.Max(0.0, baseRect.Width - newWidth), bottomRightRect.Height);
            }

            // Adjust the height of the TopLeft and BottomLeft rectangles so that they are proportional to the height of the baseRect
            if (bottomLeftRect.Top < topLeftRect.Bottom)
            {
                var newHeight = (topLeftRect.Height / (topLeftRect.Height + bottomLeftRect.Height)) * baseRect.Height;
                topLeftRect = new Rect(topLeftRect.Location.X, topLeftRect.Location.Y, topLeftRect.Width, newHeight);
                bottomLeftRect = new Rect(bottomLeftRect.Location.X, baseRect.Top + newHeight, bottomLeftRect.Width, Math.Max(0.0, baseRect.Height - newHeight));
            }

            var roundedRectGeometry = new StreamGeometry();
            using (var context = roundedRectGeometry.Open())
            {
                // Begin from the Bottom of the TopLeft Arc and proceed clockwise
                context.BeginFigure(topLeftRect.BottomLeft, true, true);
                // TopLeft Arc
                context.ArcTo(topLeftRect.TopRight, topLeftRect.Size, 0, false, SweepDirection.Clockwise, true, true);
                // Top Line
                context.LineTo(topRightRect.TopLeft, true, true);
                // TopRight Arc
                context.ArcTo(topRightRect.BottomRight, topRightRect.Size, 0, false, SweepDirection.Clockwise, true, true);
                // Right Line
                context.LineTo(bottomRightRect.TopRight, true, true);
                // BottomRight Arc
                context.ArcTo(bottomRightRect.BottomLeft, bottomRightRect.Size, 0, false, SweepDirection.Clockwise, true, true);
                // Bottom Line
                context.LineTo(bottomLeftRect.BottomRight, true, true);
                // BottomLeft Arc
                context.ArcTo(bottomLeftRect.TopLeft, bottomLeftRect.Size, 0, false, SweepDirection.Clockwise, true, true);
            }

            return roundedRectGeometry;
        }
    }
}