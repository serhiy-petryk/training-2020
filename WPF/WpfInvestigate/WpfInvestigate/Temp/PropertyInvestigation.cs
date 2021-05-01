using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WpfInvestigate.Common;

namespace WpfInvestigate.Temp
{
    public class PropertyInvestigation
    {
        public static Dictionary<Type, List<PropertyInfo>> pis = new Dictionary<Type, List<PropertyInfo>>();
        public static Dictionary<Type, List<PropertyDescriptor>> pds = new Dictionary<Type, List<PropertyDescriptor>>();
        public static Dictionary<Type, List<DependencyProperty>> dps = new Dictionary<Type, List<DependencyProperty>>();
        public static Dictionary<Type, List<DependencyPropertyDescriptor>> dpds = new Dictionary<Type, List<DependencyPropertyDescriptor>>();

        public static void UpdateProperties(DependencyObject root = null)
        {
            // var a1 = Tips.GetVisualParents(element).OfType<MwiStartup>().FirstOrDefault();
            // var elements = GetVChildren(a1).Union(new[] { a1 }).ToArray();
            // Debug.Print($"Elements count: {elements.Length}");

            pis.Clear();
            pds.Clear();
            dps.Clear();
            dpds.Clear();
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

            pi.AddRange(o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public));

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
