using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DGCore.DGVList
{

  public interface IDGVList_GroupItem
  {
    int Level { get; }
    int ItemCount { get; }
    int ExpandedItemCount { get; }
    bool IsExpanded { get; set; }
    void SetTotalsProperties(Misc.TotalLine[] totals);
    void FillChildList(List<object> itemList);
  }

  //=============  DGVList_GroupItem<T>  ==============
  public class DGVList_GroupItem<T> : IDGVList_GroupItem, Common.IGetValue
  {

    public DGVList_GroupItem<T> _parent;
    //      public T _item;
    public List<DGVList_GroupItem<T>> _childGroups;
    public List<T> _childItems;
    string _propertyName;
    public object _propertyValue;
    //      Dictionary<string, object> _properties = new Dictionary<string, object>();
    public bool _isExpanded = true;
    double[] _totalValues;
    Misc.TotalLine[] _totalDefintions;
    int[] _totalItemCount; //???
    PropertyDescriptorCollection _pdc;

    public DGVList_GroupItem() { }
    // Create wrapper for group element
    public DGVList_GroupItem<T> CreateChildGroup(string groupValueName, object groupValue, bool isExpanded, PropertyDescriptorCollection pdc)
    {
      if (this._childGroups == null) this._childGroups = new List<DGVList_GroupItem<T>>();
      DGVList_GroupItem<T> newItem = new DGVList_GroupItem<T>();
      newItem._parent = this;
      newItem._pdc = pdc;
      newItem._propertyName = groupValueName;
      newItem._propertyValue = groupValue;
      newItem._totalDefintions = this._totalDefintions;
      newItem._isExpanded = isExpanded;// Level 1 == already initially expanded
      this._childGroups.Add(newItem);
      return newItem;
    }
    //============
    public double[] TotalValues
    {
      get
      {
        if (this._totalValues == null) GetTotals();
        return this._totalValues;
      }
    }
    public void ResetTotals()
    {
      if (this._totalValues != null) this._totalValues = null;
    }

    public bool IsVisible
    {
      get
      {
        //if (Level == 0) return false;
        return _parent == null ? true : this._parent.IsVisible && this._parent._isExpanded;
      }
    }
    public int Level
    {
      get { return _parent == null ? 0 : this._parent.Level + 1; }
    }
    public int ItemCount
    {
      get
      {
        if (this._childItems != null) return this._childItems.Count;
        try
        {
          return System.Linq.Enumerable.Sum<DGVList_GroupItem<T>>(_childGroups,
            (Func<DGVList_GroupItem<T>, int>) delegate(DGVList_GroupItem<T> grItem) { return grItem.ItemCount; });
        }
        catch (Exception ex)
        {
          throw new Exception("LOVUSHKA!!!" +Environment.NewLine + ex);
        }
      }
    }
    public bool IsEmpty
    {
      get
      {
        if (this._childGroups == null) return this._childItems.Count == 0;
        foreach (DGVList_GroupItem<T> item in this._childGroups)
        {
          if (!item.IsEmpty) return false;
        }
        return true;
      }
    }
    public bool IsExpanded
    {
      get { return this._isExpanded; }
      set { this._isExpanded = value; }
    }
    public int ExpandedItemCount
    {
      get
      {
        if (this._isExpanded)
        {
          if (this._childGroups == null) return this._childItems.Count + 1;
          int cnt = 1;
          foreach (DGVList_GroupItem<T> o in this._childGroups) cnt += o.ExpandedItemCount;
          return cnt;
        }
        else return 1;
      }
    }

    public void FillChildList(List<object> itemList)
    {
      if (this._childItems == null)
      {
        foreach (DGVList_GroupItem<T> item in this._childGroups)
        {
          itemList.Add(item);
          if (item.IsExpanded) item.FillChildList(itemList);
        }
      }
      else
      {
        foreach (object o in this._childItems) itemList.Add(o);
      }
    }

    object GetPropertyValue(string propertyName)
    {
      if (this._propertyName == propertyName) return this._propertyValue;
      if (this._propertyValue != null && this._pdc != null && propertyName.StartsWith(this._propertyName + "."))
      {
        string s = propertyName.Substring(this._propertyName.Length + 1);
        PropertyDescriptor pd = this._pdc[s];
        if (pd != null) return pd.GetValue(this._propertyValue);
      }
      if (this._parent != null) return this._parent.GetPropertyValue(propertyName);
      return null;
    }

    public object GetValue(string propertyName)
    {
      object value = GetPropertyValue(propertyName);
      if (value == null)
      {
        if (this._totalDefintions != null)
        {
          for (int i = 0; i < this._totalDefintions.Length; i++)
          {
            if (this._totalDefintions[i].Id == propertyName)
            {
              if (this._totalValues == null) GetTotals();// Refresh totals if they do not exist
              return this._totalValues[i];
            }
          }
        }
      }
      return value;
      /*        object value = null;

              if (!this._properties.TryGetValue(propertyName, out value)) {
                if (this._totalDefintions != null) {
                  for (int i = 0; i < this._totalDefintions.Length; i++) {
                    if (this._totalDefintions[i]._pd.Name == propertyName) {
                      if (this._totalValues == null) GetTotals();// Refresh totals if they do not exist
                      return this._totalValues[i];
                    }
                  }
                }
              }
              return value;*/
    }

    // === totals =======
    public void SetTotalsProperties(Misc.TotalLine[] totalLines)
    {// Call only for root
      this._totalDefintions = totalLines;
    }

    double[] GetTotals()
    {
      if (this._totalDefintions == null || this._totalValues != null) return this._totalValues;
      this._totalValues = new double[this._totalDefintions.Length];
      this._totalItemCount = new int[this._totalDefintions.Length];
      // Init total values
      for (int i = 0; i < this._totalDefintions.Length; i++)
      {
        if (this._totalDefintions[i].TotalFunction == Common.Enums.TotalFunction.Minimum)
        {
          this._totalValues[i] = double.MaxValue;
        }
        else if (this._totalDefintions[i].TotalFunction == Common.Enums.TotalFunction.Maximum)
        {
          this._totalValues[i] = double.MinValue;
        }
        else this._totalValues[i] = 0;
        this._totalItemCount[i] = 0;
      }
      if (this._childGroups != null && this._childGroups.Count > 0)
      {
        for (int k = 0; k < this._childGroups.Count; k++)
        {
          DGVList_GroupItem<T> child = this._childGroups[k];
          var dd = child.GetTotals().Where(x => !double.IsNaN(x)).ToArray();
          for (var i = 0; i < dd.Length; i++)
          {
            this._totalItemCount[i] += child._totalItemCount[i];
            switch (this._totalDefintions[i].TotalFunction)
            {
              case Common.Enums.TotalFunction.First:
                if (k == 0) this._totalValues[i] = dd[i];
                break;
              case Common.Enums.TotalFunction.Last:
                if (k == (this._childGroups.Count - 1)) this._totalValues[i] = dd[i];
                break;
              case Common.Enums.TotalFunction.Count:
                this._totalValues[i] += child._totalItemCount[i];
                break;
              case Common.Enums.TotalFunction.Average:
                this._totalValues[i] += dd[i] * child._totalItemCount[i];
                break;
              case Common.Enums.TotalFunction.Sum:
                this._totalValues[i] += dd[i];
                break;
              case Common.Enums.TotalFunction.Maximum:
                this._totalValues[i] = Math.Max(this._totalValues[i], dd[i]);
                break;
              case Common.Enums.TotalFunction.Minimum:
                this._totalValues[i] = Math.Min(this._totalValues[i], dd[i]);
                break;
            }
          }
        }
      }
      if (this._childItems != null && this._childItems.Count > 0)
      {
        bool[] notFirstFlags = new bool[this._totalDefintions.Length];
        foreach (T item in this._childItems)
        {
          for (int i = 0; i < this._totalDefintions.Length; i++)
          {
            object o = this._totalDefintions[i].PropertyDescriptor.GetValue(item);
            if (o != null)
            {
              this._totalItemCount[i]++;
              switch (this._totalDefintions[i].TotalFunction)
              {
                case Common.Enums.TotalFunction.First:
                  if (!notFirstFlags[i]) this._totalValues[i] = Convert.ToDouble(o);
                  break;
                case Common.Enums.TotalFunction.Last:
                  this._totalValues[i] = Convert.ToDouble(o);
                  break;
                case Common.Enums.TotalFunction.Count:
                  this._totalValues[i] += 1.0;
                  break;
                case Common.Enums.TotalFunction.Average:
                case Common.Enums.TotalFunction.Sum:
                  this._totalValues[i] += Convert.ToDouble(o);
                  break;
                case Common.Enums.TotalFunction.Maximum:
                  this._totalValues[i] = Math.Max(this._totalValues[i], Convert.ToDouble(o));
                  break;
                case Common.Enums.TotalFunction.Minimum:
                  this._totalValues[i] = Math.Min(this._totalValues[i], Convert.ToDouble(o));
                  break;
              }
              notFirstFlags[i] = true;
            }
          }
        }
      }
      // Rounding rezult
      for (int i = 0; i < this._totalDefintions.Length; i++)
      {
        if (this._totalItemCount[i] == 0)
        {
          this._totalValues[i] = double.NaN;
        }
        else
        {
          if (this._totalDefintions[i].TotalFunction == Common.Enums.TotalFunction.Average)
          {
            this._totalValues[i] = Math.Round(this._totalValues[i] / this._totalItemCount[i], this._totalDefintions[i].DecimalPlaces);
          }
          else
          {
            this._totalValues[i] = Math.Round(this._totalValues[i], this._totalDefintions[i].DecimalPlaces);
          }
        }
      }
      return this._totalValues;
    }

  }

}

