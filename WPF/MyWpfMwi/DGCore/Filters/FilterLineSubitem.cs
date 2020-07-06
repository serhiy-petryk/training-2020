using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DGCore.Filters
{
  //===============  Class FilterLineItemCollection  ==============
  public class FilterLineSubitemCollection : BindingList<FilterLineSubitem>
  {
    List<FilterLineSubitem> _uncommittedItems = new List<FilterLineSubitem>();
    FilterLineBase _owner;

    public FilterLineSubitemCollection(FilterLineBase owner)
    {
      this._owner = owner;
    }
    protected override void InsertItem(int index, FilterLineSubitem item)
    {
      try
      {
        item.Owner = this._owner;
        base.InsertItem(index, item);
      }
      catch (Exception ex)
      {
      }
    }
  }

  //===============  Class FilterLineItem  ==============
  public class FilterLineSubitem : IDataErrorInfo
  {
    Common.Enums.FilterOperand _operand;
    FilterLineBase _owner;
    object _value1;
    object _value2;
    //      public Delegate _predicat;

    public string GetStringPresentation()
    {
      string s = GetShortStringPresentation();
      if (s == null) return null;
      else return "[" + this._owner.DisplayName + "] " + s;
    }

    public string GetShortStringPresentation()
    {// без названия поля
      if (this.IsValid)
      {
        string sOperand = TypeDescriptor.GetConverter(typeof(Common.Enums.FilterOperand)).ConvertToString(this._operand);
        int i = Common.Enums.FilterOperandTypeConverter.GetParameterQuantity(this._operand);
        if (i < 1)
        {
          return sOperand;
        }
        else if (i < 2)
        {
          string sValue1 = TypeDescriptor.GetConverter(this._value1.GetType()).ConvertToString(this._value1);
          return sOperand + " " + sValue1;
        }
        else
        {
          string sValue1 = TypeDescriptor.GetConverter(this._value1.GetType()).ConvertToString(this._value1);
          string sValue2 = TypeDescriptor.GetConverter(this._value2.GetType()).ConvertToString(this._value2);
          return sOperand + " " + sValue1 + " i " + sValue2;
        }
      }
      return null;
    }

    [DisplayName("Операнд")]
    public Common.Enums.FilterOperand FilterOperand
    {
      get
      {
        return this._operand;
      }
      set
      {
        this._operand = value;
        int i = Common.Enums.FilterOperandTypeConverter.GetParameterQuantity(this._operand);
        if (i < 2) this._value2 = null;
        if (i < 1) this._value1 = null;
      }
    }

    public object Value1
    {
      get
      {
        return this._value1;
      }
      set
      {
        if (this.Owner is FilterLine_Item)
          this._value1 = Utils.Tips.ConvertTo(value, this.Owner.PropertyType, ((FilterLine_Item)this.Owner)._pd.Converter);
        else
          this._value1 = Utils.Tips.ConvertTo(value, this.Owner.PropertyType, null);
        int i = Common.Enums.FilterOperandTypeConverter.GetParameterQuantity(this._operand);
        if (this._value1 == null)
        {
          if (i > 0) this._operand = Common.Enums.FilterOperand.None;
        }
        else
        {
          if (i < 1) this._operand = Common.Enums.FilterOperand.Equal;
        }
      }
    }
    public object Value2
    {
      get { return this._value2; }
      set
      {
        if (this.Owner is FilterLine_Item)
          this._value2 = Utils.Tips.ConvertTo(value, this._owner.PropertyType, ((FilterLine_Item)this.Owner)._pd.Converter);
        else
          this._value2 = Utils.Tips.ConvertTo(value, this._owner.PropertyType, null);
        int i = Common.Enums.FilterOperandTypeConverter.GetParameterQuantity(this._operand);
        if (this._value2 == null)
        {
          if (i == 2) this._operand = Common.Enums.FilterOperand.Equal;
        }
        else
        {
          if (i < 2) this._operand = Common.Enums.FilterOperand.Between;
        }
      }
    }
    [Browsable(false)]
    public FilterLineBase Owner
    {
      get { return this._owner; }
      set { this._owner = value; }
    }

    [Browsable(false)]
    public string Error
    {
      get
      {
        StringBuilder sb = new StringBuilder();
        int i = Common.Enums.FilterOperandTypeConverter.GetParameterQuantity(this._operand);
        if (i > 0 && this._value1 == null) sb.Append("Вкажіть вираз №1. ");
        if (i < 1 && this._value1 != null) sb.Append("Зітріть вираз №1");
        if (i > 1 && this._value2 == null) sb.Append("Вкажіть вираз №2.");
        if (i < 2 && this._value2 != null) sb.Append("Зітріть вираз №2");
        return sb.ToString();
      }
    }

    [Browsable(false)]
    public string this[string columnName]
    {
      get
      {
        int i = Common.Enums.FilterOperandTypeConverter.GetParameterQuantity(this._operand);
        if (i > 0 && this._value1 == null && columnName == "Value1") return "Вкажіть вираз №1";
        if (i < 1 && this._value1 != null && columnName == "Value1") return "Зітріть вираз №1";
        if (i > 1 && this._value2 == null && columnName == "Value2") return "Вкажіть вираз №2";
        if (i < 2 && this._value2 != null && columnName == "Value2") return "Зітріть вираз №2";
        return null;
      }
    }

    [Browsable(false)]
    public bool IsValid
    {
      get { return this._operand != Common.Enums.FilterOperand.None && String.IsNullOrEmpty(this.Error); }
    }
    [Browsable(false)]
    public bool IsError
    {
      get { return !String.IsNullOrEmpty(this.Error); }
    }

    public override string ToString() => GetStringPresentation();
  }

}
