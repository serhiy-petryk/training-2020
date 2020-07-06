using System;
using System.Collections;
using System.Collections.Generic;
using DataCommon.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestingDataX64.WiP
{
  [TestClass]
  public class Class1
  {
    [TestMethod]
    public void Test()
    {
      var a1 = ClassX1.InitActions;
      var a2 = ClassX2.InitActions;

      var a3 = new ClassX3<ClassX1>();
      var a31 = a3.InitActions;
      var a4 = new ClassX3<ClassX2>();
      var a41 = a4.InitActions;
    }
  }

  public class BaseClass
  {
    public static Dictionary<string, object> Getters;
    // public List<Type> DependedTypes;
    public static List<Action> InitActions;
  }

  public class ClassX1 : BaseClass
  {
    public static List<Action> InitActions = new List<Action> {ClassX2.Init, Init};
  private static void Init() { }

    /*static ClassX1()
    {
      Getters = new Dictionary<string, object> {{"A1", (Func<ClassX1, int>) (item => item.A1)}};
      InitActions = new List<Action> {ClassX2.Init, Init};
    }*/

    public int A1 { get; private set; }
  }

  public class ClassX2 : BaseClass
  {
    public static void Init() { }

    static ClassX2()
    {
      // Getters = new Dictionary<string, object>();
    }
  }

  public class ClassX3<T> where T : BaseClass
  {
    public List<Action> InitActions => BaseClass.InitActions;
  }

  //=====================
  public class XClass1 : Class1<XClass1>
  {
    public void Test()
    {
      var a1 = XClass1._getters;
    }
  }

  public class Class1<T> 
  {
    private protected static Dictionary<string, object> _getters = new Dictionary<string, object>();
    public static object GetGetter(string propertyName) => _getters[propertyName];

    private static string _sql;
    private static string _status;
    private static List<T> Data;

    protected static Action _refreshData;

    public static IEnumerable GetData()
    {
      if (Data == null)
        _refreshData();
      return Data;
    }

    /*private static void RefreshData()
    {
      _status = "init";
      Data = new List<T>();
      using (var conn = new SqlConnection(Common.csDbOneSap))
      using (var cmd = conn.CreateCommand())
      {
        cmd.CommandText = _sql;
        conn.Open();
        using (var rdr = cmd.ExecuteReader())
        {
          _status = "loading";
          var oo = new object[rdr.FieldCount];
          while (rdr.Read())
          {
            rdr.GetValues(oo);
            Data.Add(new T());
          }
        }
      }
      _status = "finished";

    }*/
  }
}
