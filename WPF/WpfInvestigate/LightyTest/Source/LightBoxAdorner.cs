using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace LightyTest.Source
{
    /// <summary>
    /// Adorner definition to display over LightBox on existing UI
    /// </summary>
    public class LightBoxAdorner : Adorner
    {
        public LightBox Root { get; private set; }

        public bool UseAdornedElementSize { get; set; }

        static LightBoxAdorner()
        {
        }

        public LightBoxAdorner(UIElement adornedElement) : base(adornedElement)
        {
            this.UseAdornedElementSize = true;
        }

        public void SetRoot(LightBox root)
        {
            this.AddVisualChild(root);
            this.Root = root;
        }

        protected override int VisualChildrenCount => Root == null ? 0 : 1;

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException();
            return this.Root;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (this.UseAdornedElementSize)
            {
                var size = this.AdornedElement.RenderSize;
                this.Root.Width = size.Width;
                this.Root.Height = size.Height;
                this.Root.Measure(size);
            }
            else
            {
                // Adjusted the root grid to be the same size as the element wearing Adorner
                this.Root.Width = constraint.Width;
                this.Root.Height = constraint.Height;
                this.Root.Measure(constraint);
            }

            return this.Root.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this.Root.Arrange(new Rect(new Point(0, 0), finalSize));
            return new Size(this.Root.ActualWidth, this.Root.ActualHeight);
        }
    }
}
