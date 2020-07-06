using System;
using System.Collections;
using System.ComponentModel;

namespace DGCore.Sql {
  
  public abstract class DataSourceBase: IComponent {//: Component {

    public enum DataEventKind { Loading, Loaded, Clear, BeforeRefresh, Refreshed };

    public class SqlDataEventArgs : EventArgs {
      public DataEventKind EventKind;
      public int RecordCount;
      public bool CancelFlag;
      public SqlDataEventArgs(DataEventKind eventKind)
      {
        EventKind = eventKind;
      }
    }

    //=================   Event section  ==================
    public delegate void dlgDataEvent(object sender, SqlDataEventArgs e);

    //================    Object  ================
    protected Type _itemType;
    PropertyDescriptorCollection _pdc;

    public event dlgDataEvent DataEventHandler;

//    protected abstract object GetKey();

    public Type ItemType => _itemType;
    public PropertyDescriptorCollection Properties => _pdc ?? (_pdc = PD.MemberDescriptorUtils.GetTypeMembers(ItemType));
    public abstract bool IsPartiallyLoaded { get; }
    public abstract bool IsDataReady { get; }
    public abstract ICollection GetData(bool requeryFlag);// ICollection: because we took the number of records in frmDGV

    protected void InvokeDataEvent(object sender, SqlDataEventArgs e)
    {
      DataEventHandler?.Invoke(sender, e);
    }

    public event EventHandler Disposed;
    public abstract void Dispose();
    public ISite Site { get; set; }
  }
}
