using System;
using System.ComponentModel;

namespace MyWpfMwi.Common
{
    public class DGListComponent : IComponent
    {
        private bool _disposing = false;
        public DGCore.DGVList.IDGVList Data;
        public void Dispose()
        {
            if (_disposing)
                return;

            _disposing = true;
            Data?.Dispose();
            Disposed?.Invoke(this, new EventArgs());
            Data = null;
        }

        public ISite Site { get; set; }
        public event EventHandler Disposed;
    }
}
