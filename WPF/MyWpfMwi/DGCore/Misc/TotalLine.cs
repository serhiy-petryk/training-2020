using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;

namespace DGCore.Misc {

  public class TotalLine: Common.ITotalLine {//: ICloneable {

    private static Type[] typesForTotalLines = {
      typeof(char), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint),
      typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)
    };

    public static bool IsTypeSupport(Type t) => typesForTotalLines.Contains(t);

    public static void ApplySettings(IEnumerable<TotalLine> target, IEnumerable<Common.ITotalLine> source)
    {
      foreach (Common.ITotalLine tl1 in source)
      foreach (TotalLine tl2 in target)
        if (tl2._id == tl1.Id)
        {
          tl2.TotalFunction = tl1.TotalFunction;
          tl2.DecimalPlaces = tl1.DecimalPlaces;
          break;
        }
    }

    //===============================
    [NonSerialized] 
    PropertyDescriptor _pd;// can be null; before work with pd you need to activate it (use PropertyDescriptor property set)
    [JsonProperty]
    string _id;
    [JsonProperty]
    string _displayName;
    [JsonProperty]
    Common.Enums.TotalFunction _totalFunction = Common.Enums.TotalFunction.None;
    [JsonProperty]
    int _dpTotals = 7;

    public TotalLine() {}

    public TotalLine(PropertyDescriptor pd)
    {
      _id = pd.Name;
      _displayName = pd.DisplayName;
      _pd = pd;
    }

    [Browsable(false)]
    public string Id => _id;

    public string DisplayName => _displayName;

    public Common.Enums.TotalFunction TotalFunction {
      get => _totalFunction;
      set => _totalFunction = value;
    }

    public int DecimalPlaces {
      get => _dpTotals;
      set => _dpTotals = Math.Min(15, Math.Max(0, value));
    }

    [Browsable(false), JsonIgnore]
    public PropertyDescriptor PropertyDescriptor
    {
      get => _pd;
      set
      {
        // _id = value.Name;
        _displayName = value.DisplayName;
        _pd = value;
      }
    }

    public UserSettings.TotalLine ToSettingsTotalLine() => new UserSettings.TotalLine{Id=_id, DecimalPlaces = _dpTotals, TotalFunction = _totalFunction};

    public override string ToString() => _id;
  }

}




