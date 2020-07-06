using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace DGCore.Utils
{
  public static class Types
  {

    public enum TypeKind { Primitive, Structure, Enum, String, Object, nullPrimitive, nullStructure, nullEnum };

    public static MethodInfo GetConvertMethodInfo(Type sourceType, Type destinationType)
    {
      string methodName = (destinationType == typeof(float) ? "ToSingle" : "To" + destinationType.Name);
      return typeof(Convert).GetMethod(methodName, new Type[] { sourceType });
    }

    public static bool IsNumberType(Type type)
    {
      Type t = GetNotNullableType(type);
      return (t == typeof(byte)) || (t == typeof(sbyte)) || (t == typeof(short)) || (t == typeof(ushort)) ||
        (t == typeof(int)) || (t == typeof(uint)) || (t == typeof(long)) || (t == typeof(ulong)) ||
        (t == typeof(decimal)) || (t == typeof(double)) || (t == typeof(float));
    }

    public static bool IsIntegerNumberType(Type type)
    {
      Type t = GetNotNullableType(type);
      return (t == typeof(byte)) || (t == typeof(sbyte)) || (t == typeof(short)) || (t == typeof(ushort)) ||
        (t == typeof(int)) || (t == typeof(uint)) || (t == typeof(long)) || (t == typeof(ulong));
    }

    public static bool IsNullableType(Type type)
    {
      return (type != null && type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
    }
    public static bool IsConvertible(Type type)
    {
      return type.GetInterface("System.IConvertible") != null;
    }
    public static Type GetNotNullableType(Type type)
    {
      if (IsNullableType(type))
        return Nullable.GetUnderlyingType(type);
      return type;
    }
    public static Type GetNullableType(Type type)
    {
      if (type.IsValueType && !IsNullableType(type))
        return typeof(Nullable<>).MakeGenericType(new Type[] { type });
      return type;
    }

    public static TypeKind GetTypeOfType(Type type)
    {
      if (type == typeof(string)) return TypeKind.String;
      if (type.IsClass) return TypeKind.Object;
      if (IsNullableType(type))
      {
        Type t = GetNotNullableType(type);
        if (t.IsEnum) return TypeKind.nullEnum;
        if (t.IsPrimitive) return TypeKind.nullPrimitive;
        return TypeKind.Structure;
      }
      else
      {
        if (type.IsEnum) return TypeKind.Enum;
        if (type.IsPrimitive) return TypeKind.Primitive;
        return TypeKind.Structure;
      }
    }
    //=====================
    public static object CastDbNullValue(object dbNullValue, Type valueType, string memberName)
    {
      if (dbNullValue == null) return null; // dbNullValue not defined == return null
      if (IsNullableType(valueType))
      {
        throw new Exception("BO_DbColumnAttribute attribute  error for member " + memberName + "." + Environment.NewLine +
          "dbNullValue parameter of BO_DbColumnAttribute attribute can be not null only for non nullable types");
      }
      if (dbNullValue.GetType() != valueType)
      {
        try
        {
          return Convert.ChangeType(dbNullValue, valueType);
        }
        catch (Exception ex)
        {
          throw new Exception("BO_DbColumnAttribute attribute error for member " + memberName + "." + Environment.NewLine +
            "dbNullValue parameter can not be converted to " + valueType.Name + " data type." + Environment.NewLine +
            "Error message: " + ex.Message);
        }
      }
      else return dbNullValue;
    }

    public static string GetTypeCSString(Type type)
    {
      string specString1 = ((char)1).ToString();
      if (type.FullName.Contains("[]"))
      {
      }
      string s1 = type.FullName.Replace("[]", specString1); //can be Array[]
      int cnt = 0;
      while (true)
      {
        cnt++;
        if (cnt == 12)
        {
        }
        int i2 = s1.IndexOf("]", StringComparison.Ordinal);
        if (i2 > 0)
        {
          int i1 = s1.Substring(0, i2).LastIndexOf("[", StringComparison.Ordinal);
          int i3 = s1.IndexOf(", ", i1, StringComparison.Ordinal);
          if (i3 > i1 && i3 < i2)
          {
            s1 = s1.Substring(0, i1) + s1.Substring(i1 + 1, i3 - i1 - 1) + s1.Substring(i2 + 1);
          }
          else
          {
            int i4 = s1.Substring(0, i1).LastIndexOf("`", StringComparison.Ordinal);
            int i5 = s1.Substring(0, i1).LastIndexOf("+", StringComparison.Ordinal);
            if (i5 > i4 && i5 < i1)
            {
              string subType = s1.Substring(i5 + 1, i1 - i5 - 1);
              s1 = s1.Substring(0, i4) + "<" + s1.Substring(i1 + 1, i2 - i1 - 1) + ">." + subType + s1.Substring(i2 + 1);
            }
            else
            {
              s1 = s1.Substring(0, i4) + "<" + s1.Substring(i1 + 1, i2 - i1 - 1) + ">" + s1.Substring(i2 + 1);
            }
          }
        }
        else
        {
          return s1.Replace("+", ".").Replace(specString1, "[]"); ;
        }
      }
    }

    public static Type TryGetType(string typeName)
    {
      Type t = Type.GetType(typeName, false, false);
      if (t != null) return t;
      int i = typeName.IndexOf(",", StringComparison.Ordinal);
      string name = typeName;
      string assemblyName = null;
      if (i >= 0)
      {
        name = typeName.Substring(0, i).Trim();
        assemblyName = typeName.Substring(i + 1).Trim();
      }

      List<string> ss = new List<string>();
      Assembly[] aLoaded = Thread.GetDomain().GetAssemblies();
      // Searching from last loaded (A few copies of one assembly can be in Design Mode)
      for (int i1 = (aLoaded.Length - 1); i1 >= 0; i1--)
      {
        Assembly a = aLoaded[i1];
        string assemblyKey = a.GetName().Name;
        if (!ss.Contains(assemblyKey))
        {
          t = TryGetTypeFromAssembly(name, assemblyName, a);
          if (t != null) return t;
          //          ss.Add(assemblyKey);
        }
      }
      /*      foreach (Assembly a in aLoaded) {
              string assemblyKey  =a.GetName().Name;
              if (!ss.Contains(assemblyKey)) {
                t = TryGetTypeFromAssembly(name, assemblyName, a);
                if (t != null) return t;
                ss.Add(assemblyKey);
              }
            }*/
      return null;
    }

    private static Type TryGetTypeFromAssembly(string userTypeName, string userAssemblyName, Assembly assembly)
    {
      if (String.IsNullOrEmpty(userTypeName)) return null;
      string assName = assembly.GetName().Name;
      if (String.IsNullOrEmpty(userAssemblyName) || assName == userAssemblyName || assName.StartsWith(userAssemblyName + ","))
      {
        Type type = assembly.GetType(userTypeName, false, false);
        if (type != null) return type;
      }
      return null;
    }

  }
}
