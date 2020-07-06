using System;
using System.Collections.Generic;

namespace DGCore.Menu
{
  public partial class RootMenu
  {
    public class MainObject
    {
      public string Title { get; set; }
      public Dictionary<string, DbConnection> DbConnections { get; } = new Dictionary<string, DbConnection>(StringComparer.OrdinalIgnoreCase);
      public Dictionary<string, Lookup> Lookups { get; } = new Dictionary<string, Lookup>(StringComparer.OrdinalIgnoreCase);
      public Dictionary<string, object> Menu { get; set; }
      internal List<MenuOption> FlatMenu { get; } = new List<MenuOption>();

      public void Normalize()
      {
        SetFlatMenu();
        Title = Title?.Trim();

        foreach (var o in Lookups.Values)
          o.Normalize(this);

        foreach (var mo in FlatMenu)
          mo.Normalize(this);
      }

      //=======================================
      private void SetFlatMenu()
      {
        FlatMenu.Clear();
        foreach (var kvp in Menu)
          SetFlatMenuRecursive(null, kvp);
      }

      private void SetFlatMenuRecursive(MenuOption parent, KeyValuePair<string, object> kvp)
      {
        var mo = ((Newtonsoft.Json.Linq.JObject) kvp.Value).ToObject<MenuOption>();
        mo.Label = kvp.Key;
        mo.ParentId = parent?.Id;
        FlatMenu.Add(mo);
        if (!mo.IsSubmenu) return;
        // Submenu
        var o2 = ((Newtonsoft.Json.Linq.JObject) kvp.Value).ToObject<Dictionary<string, object>>();
        foreach (var kvp1 in o2)
          SetFlatMenuRecursive(mo, kvp1);
      }
    }
  }
}
