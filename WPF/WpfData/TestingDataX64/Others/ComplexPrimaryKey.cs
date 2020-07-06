using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using DataCommon.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace TestingDataX64.Others
{
  [TestClass]
  public class ComplexPrimaryKey
  {
    [TestMethod]
    public void TestComplexPrimaryKey()
    {
      var r1 = Common.MemoryUsedInKB;

      var dd1 = new Dictionary<Tuple<string, int>, object>();
      for (var k = 0; k < 1000000; k++)
        dd1.Add(new Tuple<string, int>(null, k), null);
      var r2 = Common.MemoryUsedInKB;

      var sw = new Stopwatch();
      sw.Start();

      for (var k = 0; k < 1000000; k += 10)
      {
        var K = dd1[new Tuple<string, int>(null, k)];
      }

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r21 = r2 - r1;
    }

    [TestMethod]
    public void TestIntValueTuplePrimaryKey()
    {
      var r1 = Common.MemoryUsedInKB;

      var dd1 = new Dictionary<(string, int), object>();
      for (var k = 0; k < 1000000; k++)
        dd1.Add((null, k), null);
      var r2 = Common.MemoryUsedInKB;

      var sw = new Stopwatch();
      sw.Start();

      for (var k = 0; k < 1000000; k += 10)
      {
        var K = dd1[(null, k)];
      }

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r21 = r2 - r1;
    }

    [TestMethod]
    public void TestInt2ValueTuplePrimaryKey()
    {
      var r1 = Common.MemoryUsedInKB;

      var dd1 = new Dictionary<(int, string), object>();
      for (var k = 0; k < 1000000; k++)
        dd1.Add((k, null), null);
      var r2 = Common.MemoryUsedInKB;

      var sw = new Stopwatch();
      sw.Start();

      for (var k = 0; k < 1000000; k += 10)
      {
        var K = dd1[(k, null)];
      }

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r21 = r2 - r1;
    }

    [TestMethod]
    public void TestStringTuplePrimaryKey()
    {
      var r1 = Common.MemoryUsedInKB;

      var dd1 = new Dictionary<Tuple<string, int>, object>();
      for (var k = 0; k < 1000000; k++)
        dd1.Add(new Tuple<string, int>(k + "a", k), null);
      var r2 = Common.MemoryUsedInKB;

      var sw = new Stopwatch();
      sw.Start();

      for (var k = 0; k < 1000000; k += 10)
      {
        var K = dd1[new Tuple<string, int>(k + "a", k)];
      }

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r21 = r2 - r1;
    }

    [TestMethod]
    public void TestStringValueTuplePrimaryKey()
    {
      var r1 = Common.MemoryUsedInKB;

      var dd1 = new Dictionary<(string, int), object>();
      for (var k = 0; k < 1000000; k++)
        dd1.Add((k + "a", k), null);
      var r2 = Common.MemoryUsedInKB;

      var sw = new Stopwatch();
      sw.Start();

      for (var k = 0; k < 1000000; k += 10)
      {
        var K = dd1[(k + "a", k)];
      }

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r21 = r2 - r1;
    }

    [TestMethod]
    public void TestString2ValueTuplePrimaryKey()
    {
      var r1 = Common.MemoryUsedInKB;

      var dd1 = new Dictionary<(int,string), object>();
      for (var k = 0; k < 1000000; k++)
        dd1.Add((k, k + "a"), null);
      var r2 = Common.MemoryUsedInKB;

      var sw = new Stopwatch();
      sw.Start();

      for (var k = 0; k < 1000000; k += 10)
      {
        var K = dd1[(k, k + "a")];
      }

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r21 = r2 - r1;
    }

    [TestMethod]
    public void TestDoubleDictionaryPrimaryKey()
    {
      var r1 = Common.MemoryUsedInKB;

      var dd1 = new Dictionary<string, Dictionary<int, object>>();
      for (var k = 0; k < 1000000; k++)
      {
        var dd2=new Dictionary<int, object>{{k,null}};
        dd1.Add(k+"a", dd2);
      }

      var r2 = Common.MemoryUsedInKB;

      var sw = new Stopwatch();
      sw.Start();

      for (var k = 0; k < 1000000; k += 10)
      {
        var K = dd1[k + "a"][k];
      }

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r21 = r2 - r1;
    }

    [TestMethod]
    public void TestIntPrimaryKey()
    {
      var r1 = Common.MemoryUsedInKB;

      var dd1 = new Dictionary<int, object>();
      for (var k = 0; k < 1000000; k++)
        dd1.Add(k, null);
      var r2 = Common.MemoryUsedInKB;

      var sw = new Stopwatch();
      sw.Start();

      for (var k = 0; k < 1000000; k += 10)
      {
        var K = dd1[k];
      }

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r21 = r2 - r1;
    }

    [TestMethod]
    public void TestStringPrimaryKey()
    {
      var r1 = Common.MemoryUsedInKB;

      var dd1 = new Dictionary<string, object>();
      for (var k = 0; k < 1000000; k++)
        dd1.Add(k+"a", null);
      var r2 = Common.MemoryUsedInKB;

      var sw = new Stopwatch();
      sw.Start();

      for (var k = 0; k < 1000000; k += 10)
      {
        var K = dd1[k+"a"];
      }

      sw.Stop();
      var a1 = sw.Elapsed.TotalMilliseconds;
      var r21 = r2 - r1;
    }
  }
}
