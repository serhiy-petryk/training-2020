using System;
using System.Collections.Generic;

namespace DGCore.Menu
{
  public class SqlObject
  {
    public string CS { get; set; }
    public RootMenu.DbConnection oCS { get; set; }
    public string Sql { get; set; }
    public Dictionary<string, DbParameter> Parameters { get; } = new Dictionary<string, DbParameter>(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, RootMenu.Column> Columns { get; } = new Dictionary<string, RootMenu.Column>(StringComparer.OrdinalIgnoreCase);
    public string SqlForColumnAttributes { get; set; }
    public string ItemType { get; set; }
    public Type oItemType { get; set; }
    public void Normalize(RootMenu.MainObject mo)
    {
      if (!string.IsNullOrEmpty(CS))
        oCS = mo.DbConnections[CS.Trim()];

      if (!string.IsNullOrEmpty(ItemType))
      {
        oItemType = Utils.Types.TryGetType(ItemType);
        if (oItemType == null)
          throw new Exception($"Can not find item type: {ItemType}");
      }

      foreach (var kvp in Parameters)
        kvp.Value.Normalize(kvp.Key, mo);

      foreach (var kvp in Columns)
        kvp.Value.Normalize(kvp.Key, mo);

    }

  }

}
