using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DGCore.Misc {
  public static class DependentObjectManager {

    public static event EventHandler ObjectListChanged;

    public static void Bind(object producer, IComponent consumer)
    {
      if (consumer == null)
        return;
      lock (_links) {
        List<IComponent> oo;
        if (!_links.TryGetValue(producer, out oo)) {
          oo = new List<IComponent>();
          _links.Add(producer, oo);
        }
        if (!oo.Contains(consumer)) {
          oo.Add(consumer);
          consumer.Disposed += Consumer_Disposed;
/*          if (consumer is DB.DbCmd) {
            consumer.Dispose();
          }*/
        }
      }
      ObjectListChanged?.Invoke(null, new EventArgs());
    }

    public static TProducerType[] GetProducerList<TProducerType>() {
      List<TProducerType> oo = new List<TProducerType>();
      foreach (object o in _links.Keys) {
        if (o is TProducerType) oo.Add((TProducerType)o);
      }
      return oo.ToArray();
    }

    public static Dictionary<object, List<IComponent>> LinksData => _links;

    //=============    Private section  ==================
    private static Dictionary<object, List<IComponent>> _links = new Dictionary<object, List<IComponent>>();

    private static void Consumer_Disposed(object sender, EventArgs e) {
      IComponent consumer = (IComponent)sender;
      consumer.Disposed -= new EventHandler(Consumer_Disposed);
      lock (_links) {
        RemoveConsumer(consumer);
      }
      if (ObjectListChanged != null) ObjectListChanged.Invoke(null, new EventArgs());
    }

    private static void RemoveConsumer(IComponent consumer) {
      Utils.Events.RemoveAllEventSubsriptions(consumer);

      List<object> blankEntries = new List<object>();
      // Remove consumer from all dictionary values
      foreach (KeyValuePair<object, List<IComponent>> kvp in _links) {
        while (kvp.Value.Contains(consumer)) {
          kvp.Value.Remove(consumer);
          Utils.Events.RemoveEventSubsriptions(kvp.Key, consumer);
        }
        if (kvp.Value.Count == 0) blankEntries.Add(kvp.Key);
      }
      for (int i = 0; i < blankEntries.Count; i++) {
        RemoveProducerEntry(blankEntries[i]);
      }
    }

    private static void RemoveProducerEntry(object producer) {
      _links.Remove(producer);
      if (producer is IComponent) {
        IComponent consumer = (IComponent)producer;
        consumer.Disposed -= Consumer_Disposed;
        consumer.Dispose();
        RemoveConsumer(consumer);
      }
    }
  }
}
