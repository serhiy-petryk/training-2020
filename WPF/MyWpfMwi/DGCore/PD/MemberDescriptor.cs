using System;
using System.ComponentModel;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace DGCore.PD
{

  public enum MemberKind { Field = 0, Property = 1, Method = 2 };
  public delegate object GetHandler(object source);
  public delegate void SetHandler(object source, object value);
  public delegate object ConstructorHandler();


  public interface IMemberDescriptor
  {
    MemberKind MemberKind { get; }
    object DbNullValue { get; }
    MemberInfo ReflectedMemberInfo { get; }
    Delegate NativeGetter { get; }
    string Format { get; }
    System.Drawing.ContentAlignment? Alignment { get; }
  }

  //======================================
  public class MemberDescriptor<T> : PropertyDescriptor, IMemberDescriptor
  {

    public readonly string _path;
    public readonly MemberElement _member;
    private readonly string[] _members;

    string _dbColumnName;
    string _format; // for DGV
    System.Drawing.ContentAlignment? _alignment;
    object _dbNullValue;
    TypeConverter _thisConverter;

    //    TypeConverter _objectConverter;// Converter from DataBase simple type to object type

    public MemberDescriptor(string path)
      : base(path, new Attribute[0])
    {
      _path = path;
      _members = path.Split('.');
      List<string> ss = new List<string>(path.Split('.'));
      _member = new MemberElement(null, typeof(T), ss);
      base.AttributeArray = _member.Attributes;

      BO_DbColumnAttribute a = (BO_DbColumnAttribute)Attributes[typeof(BO_DbColumnAttribute)];
      if (a != null)
      {
        _dbColumnName = a._dbColumnName;
        _format = a._format;
        //        this._alignment = a._alignment;
        _dbNullValue = a._dbNullValue;
        _dbNullValue = Utils.Types.CastDbNullValue(_dbNullValue, PropertyType, ComponentType.Name + "." + PropertyType.Name);
        if (_dbNullValue != null)
        {
          TypeConverter tc = base.Converter;
          _thisConverter = PD.ConverterWithNonStandardDefaultValue.GetConverter(tc, _dbNullValue);
        }
        else if (string.Equals(_format, "hex", StringComparison.OrdinalIgnoreCase))
          _thisConverter = new ByteArrayToHexStringConverter();
        else if (string.Equals(_format, "bytestoguid", StringComparison.OrdinalIgnoreCase))
          _thisConverter = new ByteArrayToGuidStringConverter();
      }

      /* not need!!! if (_thisConverter == null && this.PropertyType == typeof(string) && (path.EndsWith("UID", StringComparison.OrdinalIgnoreCase) || path.StartsWith("ICON", StringComparison.CurrentCultureIgnoreCase)))
        this._thisConverter = new ByteArrayToHexStringConverter();*/

      /*test: if (this.Name.StartsWith("ICON"))
      {
        this._thisConverter = new PD.ByteArrayToHexStringConverter();
      }*/

      /*BO_LookupTableAttribute aLookup = (BO_LookupTableAttribute)this.Attributes[typeof(BO_LookupTableAttribute)];
      if (aLookup != null) {
        if (this.PropertyType.IsClass && this.PropertyType != typeof(string)) {
          this._thisConverter = PD.LookupTableHelper.GetTypeConverter(this.PropertyType, aLookup);
        }
        else {
          throw new Exception(path + " Property of " + this.ComponentType +" type. BO_LookupTableAttribute attribute applables only for not string object properties");
        }
      }*/
    }

    public MemberKind MemberKind
    {
      get { return _member._memberKind; }
    }
    public string Format
    {
      get { return _format; }
    }
    public System.Drawing.ContentAlignment? Alignment
    {
      get { return _alignment; }
    }
    public object DbNullValue
    {
      get { return _dbNullValue; }
    }
    public MemberInfo ReflectedMemberInfo
    {
      get
      {
        if (_member._memberKind == MemberKind.Property)
        {
          return _member._memberInfo.ReflectedType.GetProperty(Name);
        }
        return _member._memberInfo;
      }
    }

    public Delegate NativeGetter
    {
      get { return _member._nativeGetter; }
    }

    // ===================    OrderBy Section ============================
    public IEnumerable GroupBy(IEnumerable<T> source)
    {
      MethodInfo mi = MemberDescriptorUtils.GenericGroupByMi.MakeGenericMethod(new Type[] { typeof(T), _member._nativeGetter.Method.ReturnType });
      return (IEnumerable)mi.Invoke(null, new object[] { source, _member._nativeGetter });
    }
    //========================  Public section  ==============================
    /*    public int MemberLevel {
          get { return this._token._memberLevel; }
        }*/

    // ====================  Override section   =======================
    /*public override void AddValueChanged(object component, EventHandler handler) {
      base.AddValueChanged(component, handler);
    }
    public override void RemoveValueChanged(object component, EventHandler handler) {
      base.RemoveValueChanged(component, handler);
    }*/
    public override bool SupportsChangeEvents
    {
      get { return false; }
    }

    public override TypeConverter Converter
    {
      get
      {
        object o = _thisConverter ?? base.Converter;
        return _thisConverter ?? base.Converter;
      }
    }
    public sealed override string Name
    {
      get { return string.Join(".", _members); }
    }
    public override Type ComponentType
    {
      get { return _member._instanceType; }
    }
    public override Type PropertyType
    {
      get
      {
        return _member._lastNullableReturnType;
        //        return this._token._lastReturnType; 
      }
    }
    public override object GetValue(object component)
    {
      if (Utils.Tips.IsDesignMode)
      {
        return Activator.CreateInstance(_member._lastReturnType);
      }
      if (component is Common.IGetValue)
      {
        return ((Common.IGetValue)component).GetValue(Name);
      }
      return _member._getter(component);
      //      return this._getter_CD(component);
    }

    public override void SetValue(object component, object value)
    {
      _member._setter(component, value == DBNull.Value ? _dbNullValue : value);

      /*      object oldValue = this.GetValue(component);
            object newValue = (value == DBNull.Value ? this._dbNullValue : value);
            this._member._setter(component, newValue);*/
      /*      if (this._token._fi != null) {
              this._token._fi.SetValue(component, newValue);
            }
            if (this._token._pi != null) {
              this._token._pi.SetValue(component, newValue, null);
            }*/
      /*      OnValueChanged x = this.OnValueChangedHandler;
            if (x != null) {
              x(this, component, newValue, oldValue);
            }*/
    }

    public override bool IsBrowsable
    {
      get
      {
        if (_member._getter == null) return false;
        return base.IsBrowsable;
      }
    }

    public override bool IsReadOnly
    {
      get { return _member._setter == null; }
    }

    private object DefaultValue
    {
      get
      {
        DefaultValueAttribute attribute = (DefaultValueAttribute)Attributes[typeof(DefaultValueAttribute)];
        if (attribute != null) return attribute.Value;
        else return null;
      }
    }

    public override string ToString()
    {
      return typeof(T).Name + "." + Name;
    }

    public override bool CanResetValue(object component)
    {
      // Taken from System.ComponentModel.TypeConverter+SimplePropertyDescriptor
      DefaultValueAttribute attribute = (DefaultValueAttribute)Attributes[typeof(DefaultValueAttribute)];
      if (attribute == null)
      {
        return false;
      }
      return attribute.Value.Equals(GetValue(component));
    }
    public override void ResetValue(object component)
    {
      // Taken from System.ComponentModel.TypeConverter+SimplePropertyDescriptor
      DefaultValueAttribute attribute = (DefaultValueAttribute)Attributes[typeof(DefaultValueAttribute)];
      if (attribute != null)
      {
        SetValue(component, attribute.Value);
      }
    }
    public override bool ShouldSerializeValue(object component)
    {
      //      return false;
      return true;
    }


  }
}
