using System;
using System.Windows;
using System.Windows.Documents;

namespace ItemsControlDragDrop.Code
{
    public abstract class DropTargetAdorner : Adorner
    {
        public DropTargetAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            this.m_AdornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            this.m_AdornerLayer.Add(this);
            this.IsHitTestVisible = false;
        }

        public void Detatch()
        {
            this.m_AdornerLayer.Remove(this);
        }

        private readonly AdornerLayer m_AdornerLayer;
    }
}