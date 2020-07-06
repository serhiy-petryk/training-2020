using System;
using System.ComponentModel;

namespace DGCore.Sql
{
  class Test
  {
  }

  //  [ComVisible(true), Designer("System.Windows.Forms.Design.ComponentDocumentDesigner, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(IRootDesigner)), DesignerCategory("Component"), TypeConverter(typeof(ComponentConverter))]
  public class MarshalByValueComponent : IComponent, IDisposable
  {// , IServiceProvider {
    // Fields
    private static readonly object EventDisposed = new object();
    private EventHandlerList events;
    private ISite site;

    // Events
    public event EventHandler Disposed
    {
      add
      {
        this.Events.AddHandler(EventDisposed, value);
      }
      remove
      {
        this.Events.RemoveHandler(EventDisposed, value);
      }
    }

    // Methods
    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        lock (this)
        {
          if ((this.site != null) && (this.site.Container != null))
          {
            this.site.Container.Remove(this);
          }
          if (this.events != null)
          {
            EventHandler handler = (EventHandler)this.events[EventDisposed];
            if (handler != null)
            {
              handler(this, EventArgs.Empty);
            }
          }
        }
      }
    }

    ~MarshalByValueComponent()
    {
      this.Dispose(false);
    }

    /*public virtual object GetService(Type service) {
      if (this.site != null) {
        return this.site.GetService(service);
      }
      return null;
    }*/

    public override string ToString()
    {
      ISite site = this.site;
      if (site != null)
      {
        return (site.Name + " [" + base.GetType().FullName + "]");
      }
      return base.GetType().FullName;
    }

    // Properties
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public virtual IContainer Container => site?.Container;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public virtual bool DesignMode
    {
      get
      {
        ISite site = this.site;
        return ((site != null) && site.DesignMode);
      }
    }

    protected EventHandlerList Events
    {
      get
      {
        if (this.events == null)
        {
          this.events = new EventHandlerList();
        }
        return this.events;
      }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public virtual ISite Site
    {
      get => site;
      set => site = value;
    }
  }



}
