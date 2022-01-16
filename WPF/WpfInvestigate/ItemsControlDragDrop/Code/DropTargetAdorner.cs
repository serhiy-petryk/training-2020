using System.Windows;
using System.Windows.Documents;

namespace ItemsControlDragDrop.Code
{
    public abstract class DropTargetAdorner : Adorner
    {
        public DropTargetAdorner(UIElement adornedElement) : base(adornedElement)
        {
            m_AdornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            m_AdornerLayer.Add(this);
            IsHitTestVisible = false;
        }

        public void Detach() => m_AdornerLayer.Remove(this);

        private readonly AdornerLayer m_AdornerLayer;
    }
}