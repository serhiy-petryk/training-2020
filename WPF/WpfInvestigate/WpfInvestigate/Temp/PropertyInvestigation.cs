using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace WpfInvestigate.Temp
{
    public class PropertyInvestigation
    {
        public static Dictionary<Type, PropertyDescriptor[]> pds2 = new Dictionary<Type, PropertyDescriptor[]>();

        public static Dictionary<Type, List<PropertyInfo>> pis = new Dictionary<Type, List<PropertyInfo>>();
        public static Dictionary<Type, List<PropertyDescriptor>> pds = new Dictionary<Type, List<PropertyDescriptor>>();
        public static Dictionary<Type, List<DependencyProperty>> dps = new Dictionary<Type, List<DependencyProperty>>();
        public static Dictionary<Type, List<DependencyPropertyDescriptor>> dpds = new Dictionary<Type, List<DependencyPropertyDescriptor>>();
        public static Dictionary<Type, List<LocalValueEntry>> lves = new Dictionary<Type, List<LocalValueEntry>>();
        public static List<object> oo = new List<object>();
        public static List<Hashtable> oos = new List<Hashtable>();

        public static void UpdateProperties3(DependencyObject root = null)
        {
            // root = new ContainerVisual();
            var watch = Stopwatch.StartNew();
            var aa2 = new Dictionary<Type, List<List<PropertyDescriptor>>>();
            var aa3 = new Dictionary<Type, Tuple<int, int>>();
            var aa4 = new Dictionary<PropertyDescriptor, Dictionary<Type, int>>();

            pds2.Clear();

            var elementsCount = 0;
            var propertiesCount = 0;
            var rootList = root == null ? Application.Current.Windows.OfType<Window>().ToArray() : new[] { root };
            foreach (var item in rootList)
            {
                var elements = GetVChildren(item).Union(new[] { item }).ToArray();
                elementsCount += elements.Length;
                Debug.Print($"Elements count: {elements.Length}");
                foreach (var element in elements)
                {
                    var aa = GetProperties2(element).Where(a=>a.Item1.Name == "BaseUriHelper.BaseUri").ToArray();
                    /*foreach(var z in aa.Where(a => a.Item1.Name.Contains(".")).Select(a => a.Item1))
                        aa2.Add(z);*/
                    if (!aa2.ContainsKey(element.GetType()))
                    {
                        aa2.Add(element.GetType(), new List<List<PropertyDescriptor>>());
                        aa3.Add(element.GetType(), new Tuple<int, int>(0,0));
                    }

                    var xx = aa.Where(a => a.Item1.Name.Contains(".")).Select(a => a.Item1).ToList();
                    aa2[element.GetType()].Add(xx);
                    var tt = aa3[element.GetType()];
                    if (tt.Item1 == 0)
                        aa3[element.GetType()] = new Tuple<int, int>(xx.Count, xx.Count);
                    else if (xx.Count < tt.Item1)
                        aa3[element.GetType()] = new Tuple<int, int>(xx.Count, tt.Item2);
                    else if (xx.Count > tt.Item2)
                        aa3[element.GetType()] = new Tuple<int, int>(tt.Item1, xx.Count);

                    foreach (var x in xx)
                    {
                        if (!aa4.ContainsKey(x))
                            aa4.Add(x, new Dictionary<Type, int>());
                        var x1 = aa4[x];
                        if (!x1.ContainsKey(element.GetType()))
                            x1.Add(element.GetType(), 0);
                        x1[element.GetType()]++;
                    }
                    propertiesCount += aa.Length;
                }
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Debug.Print($"UpdateProperties2: elements{elementsCount}, properties: {propertiesCount}, time: {elapsedMs}");
        }

        public static void UpdateProperties2(DependencyObject root = null)
        {
            var watch = Stopwatch.StartNew();

            pds2.Clear();

            var elementsCount = 0;
            var propertiesCount = 0;
            var rootList = root == null ? Application.Current.Windows.OfType<Window>().ToArray() : new[] { root };
            foreach (var item in rootList)
            {
                var elements = GetVChildren(item).Union(new[] { item }).ToArray();
                elementsCount += elements.Length;
                Debug.Print($"Elements count: {elements.Length}");
                foreach (var element in elements)
                {
                    var aa = GetProperties(element.GetType());
                    propertiesCount += aa.Length;
                }
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Debug.Print($"UpdateProperties2: elements{elementsCount}, properties: {propertiesCount}, time: {elapsedMs}");
        }

        private static Attribute[] _attrs = {new PropertyFilterAttribute(PropertyFilterOptions.All)};
        private static Tuple<PropertyDescriptor, DependencyPropertyDescriptor>[] GetProperties(Type type)
        {
            return TypeDescriptor.GetProperties(type, _attrs).OfType<PropertyDescriptor>()
                .Select(a => new Tuple<PropertyDescriptor, DependencyPropertyDescriptor>(a, DependencyPropertyDescriptor.FromProperty(a))).ToArray();
            // return TypeDescriptor.GetProperties(type, _attrs).OfType<PropertyDescriptor>().ToArray();
            /*if (!pds2.ContainsKey(type))
            {
                pds2[type] = TypeDescriptor.GetProperties(type, attrs).OfType<PropertyDescriptor>().ToArray();
            }
            return pds2[type];*/
        }

        private static Tuple<PropertyDescriptor, DependencyPropertyDescriptor>[] GetProperties2(object o)
        {
            return TypeDescriptor.GetProperties(o, _attrs).OfType<PropertyDescriptor>()
                .Select(a => new Tuple<PropertyDescriptor, DependencyPropertyDescriptor>(a, DependencyPropertyDescriptor.FromProperty(a))).ToArray();
            // return TypeDescriptor.GetProperties(type, _attrs).OfType<PropertyDescriptor>().ToArray();
            /*if (!pds2.ContainsKey(type))
            {
                pds2[type] = TypeDescriptor.GetProperties(type, attrs).OfType<PropertyDescriptor>().ToArray();
            }
            return pds2[type];*/
        }

        public static void UpdateProperties(DependencyObject root = null)
        {
            // var a1 = Tips.GetVisualParents(element).OfType<MwiStartup>().FirstOrDefault();
            // var elements = GetVChildren(a1).Union(new[] { a1 }).ToArray();
            // Debug.Print($"Elements count: {elements.Length}");

            string[] names = null;
            if (pds.Count > 0)
            {
                names = pds.SelectMany(a => a.Value).Select(a=>a.Name).Distinct().ToArray();
            }

            pis.Clear();
            pds.Clear();
            dps.Clear();
            dpds.Clear();
            lves.Clear();
            var elementCount = 0;

            var rootList = root == null ? Application.Current.Windows.OfType<Window>().ToArray() : new[] {root};
            foreach (var item in rootList)
            {
                var elements = GetVChildren(item).Union(new[] { item }).ToArray();
                elementCount += elements.Length;
                Debug.Print($"Elements count: {elements.Length}");
                foreach (var element in elements)
                {
                    FindAttachedProperties(element, pis, pds, dps, dpds);
                    foreach (var pi in element.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var value = pi.GetValue(element);
                        if (value != null && !@pi.PropertyType.IsValueType)
                            FindAttachedProperties(value, pis, pds, dps, dpds);
                    }
                }
            }

            if (names != null)
            {
                var names2 = pds.SelectMany(a => a.Value).Select(a => a.Name).Distinct().ToArray();
                var a11 = names2.Except(names).ToArray();
                var a12 = names.Except(names2).ToArray();
                var oo1 = new List<object>();
                var fis2 = typeof(TypeDescriptor).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                foreach (var fi in fis2)
                {
                    oo1.Add(fi.GetValue(null));
                }
            }

            oo.Clear();
            var fis = typeof(TypeDescriptor).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            foreach (var fi in fis)
            {
                oo.Add(fi.GetValue(null));
            }
            oos.Add((Hashtable)(oo[2] as Hashtable).Clone());
            if (oos.Count > 1)
            {
                var aa1 = oos[oos.Count - 2];
                var aa2 = oos[oos.Count - 1];
                var keys1 = aa1.Keys.OfType<Type>().ToArray();
                var keys2 = aa2.Keys.OfType<Type>().ToArray();
                var diff = keys2.Except(keys1);
            }

            Debug.Print($"Total elements count: {elementCount}");
            Debug.Print($"Properties count. pi: {pis.Count}, pd: {pds.Count}, dp: {dps.Count}, dpd: {dpds.Count}");
        }

        private static void FindAttachedProperties(object o, Dictionary<Type, List<PropertyInfo>> pis, Dictionary<Type, List<PropertyDescriptor>> pds, Dictionary<Type, List<DependencyProperty>> dps, Dictionary<Type, List<DependencyPropertyDescriptor>> dpds)
        {
            if (o == null) return;
            var t = o.GetType();
            if (pis.ContainsKey(t)) return;

            var pi = new List<PropertyInfo>();
            var pd = new List<PropertyDescriptor>();
            var dp = new List<DependencyProperty>();
            var dpd = new List<DependencyPropertyDescriptor>();
            var lve = new List<LocalValueEntry>();

            pi.AddRange(o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public));

            var aa11 = TypeDescriptor.GetProperties(o).OfType<PropertyDescriptor>().ToList();
            var aa21 = TypeDescriptor.GetProperties(o, false).OfType<PropertyDescriptor>().ToList();
            var aa22 = TypeDescriptor.GetProperties(o, true).OfType<PropertyDescriptor>().ToList();
            var aa31 = TypeDescriptor.GetProperties(o, new Attribute[] { new PropertyFilterAttribute(PropertyFilterOptions.All) }).OfType<PropertyDescriptor>().ToList();
            var aa41 = TypeDescriptor.GetProperties(o, new Attribute[] { new PropertyFilterAttribute(PropertyFilterOptions.All) }, false).OfType<PropertyDescriptor>().ToList();
            var aa42 = TypeDescriptor.GetProperties(o, new Attribute[] { new PropertyFilterAttribute(PropertyFilterOptions.All) }, true).OfType<PropertyDescriptor>().ToList();
            if (aa21.Count != aa41.Count)
            {
                var a2 = aa21.Select(a => a.Name).ToArray();
                var a4 = aa41.Select(a => a.Name).ToArray();
                var a2x = a2.Except(a4).ToArray();
                var a4x = a4.Except(a2).ToArray();
            }
            foreach (PropertyDescriptor pd2 in TypeDescriptor.GetProperties(o, new Attribute[] { new PropertyFilterAttribute(PropertyFilterOptions.All) }))
            {
                pd.Add(pd2);
                var dpd2 = DependencyPropertyDescriptor.FromProperty(pd2);
                if (dpd2 != null)
                {
                    dp.Add(dpd2.DependencyProperty);
                    dpd.Add(dpd2);
                }
            }

            if (o is DependencyObject d)
            {
                var localValueEnumerator = d.GetLocalValueEnumerator();
                while (localValueEnumerator.MoveNext())
                {
                    var current = localValueEnumerator.Current;
                    lve.Add(current);
                    //if (!current.Property.ReadOnly)
                    //  (o as DependencyObject).ClearValue(current.Property);
                }
                lves.Add(t, lve);
            }

            pis.Add(t, pi);
            pds.Add(t, pd);
            dps.Add(t, dp);
            dpds.Add(t, dpd);
        }

        private static List<DependencyProperty> GetAttachedProperties(DependencyObject obj) // ????
        {
            var result = new List<DependencyProperty>();

            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(obj, new Attribute[] { new PropertyFilterAttribute(PropertyFilterOptions.All) }))
            {
                var dpd = DependencyPropertyDescriptor.FromProperty(pd);
                if (dpd != null)
                {
                    result.Add(dpd.DependencyProperty);
                }
            }
            return result;
        }

        private static IEnumerable<DependencyObject> GetVChildren(DependencyObject current)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(current); i++)
            {
                var child = VisualTreeHelper.GetChild(current, i);

                foreach (var childOfChild in GetVChildren(child))
                    yield return childOfChild;
                yield return child;
            }
        }
    }
}
