using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfInvestigate.Helpers
{
    public static class TypeHelper
    {
        private static Dictionary<Type, Tuple<PropertyDescriptor, DependencyPropertyDescriptor>[]> _propertiesCache =
            new Dictionary<Type, Tuple<PropertyDescriptor, DependencyPropertyDescriptor>[]>();
        private static int _lastCountOfDefaultProviders;
        private static FieldInfo _fiDefaultProvider = typeof(TypeDescriptor).GetField("_defaultProviders", BindingFlags.NonPublic | BindingFlags.Static);

        public static void CheckNewProperties()
        {
            var newCount = (_fiDefaultProvider.GetValue(null) as Hashtable).Count;
            if (newCount != _lastCountOfDefaultProviders)
            {
                _lastCountOfDefaultProviders = newCount;
                _propertiesCache.Clear();
            }
        }

        private static Attribute[] _attrs = { new PropertyFilterAttribute(PropertyFilterOptions.All) };
        public static Tuple<PropertyDescriptor, DependencyPropertyDescriptor>[] GetPropertyInfos(Type type)
        {
            if (!_propertiesCache.ContainsKey(type))
            {
                var o = Activator.CreateInstance(type);
                _propertiesCache.Add(type,
                    TypeDescriptor.GetProperties(o, _attrs).OfType<PropertyDescriptor>().Select(a =>
                        new Tuple<PropertyDescriptor, DependencyPropertyDescriptor>(a,
                            DependencyPropertyDescriptor.FromProperty(a))).ToArray());
            }

            // TypeDescriptor.GetProperties(type, attrs).OfType<PropertyDescriptor>().ToArray();
            return _propertiesCache[type];
        }

        public static DependencyPropertyDescriptor[] GetAttachedProperties(Type type)
        {
            var o = new ContainerVisual();
            var aa1 = TypeDescriptor.GetProperties(o, _attrs).OfType<PropertyDescriptor>()
                .Select(a =>
                    new Tuple<PropertyDescriptor, DependencyPropertyDescriptor>(a,
                        DependencyPropertyDescriptor.FromProperty(a))).ToArray();
            var aa2 = aa1.Where(a => a.Item2 != null && a.Item2.Name.Contains("."))
                .Select(a => a.Item2.DependencyProperty).ToArray();
            var aa3 = aa2.Select(a => DependencyPropertyDescriptor.FromProperty(a, type))
                .Where(a => a.Name.Contains(".") && a.Name == "FocusVisualEffect.FocusControlStyle").ToArray();
            return aa3;
        }

        public static DependencyPropertyDescriptor[] GetAttachedProperties(Type type, DependencyObject o2)
        {
            var o = new ContainerVisual();
            var aa1 = TypeDescriptor.GetProperties(o, _attrs).OfType<PropertyDescriptor>()
                .Select(a => new Tuple<PropertyDescriptor, DependencyPropertyDescriptor>(a, DependencyPropertyDescriptor.FromProperty(a))).ToArray();
            var aa2 = aa1.Where(a => a.Item2!=null && a.Item2.Name.Contains(".")).Select(a => a.Item2.DependencyProperty).ToArray();

            var lve = new List<LocalValueEntry>();
            var localValueEnumerator = o.GetLocalValueEnumerator();
            while (localValueEnumerator.MoveNext())
            {
                var current = localValueEnumerator.Current;
                lve.Add(current);
                //if (!current.Property.ReadOnly)
                //  (o as DependencyObject).ClearValue(current.Property);
            }
            // lves.Add(t, lve);

            var o1 = new Button();
            var lve1 = new List<LocalValueEntry>();
            var localValueEnumerator1 = o1.GetLocalValueEnumerator();
            while (localValueEnumerator1.MoveNext())
            {
                var current = localValueEnumerator1.Current;
                lve1.Add(current);
                //if (!current.Property.ReadOnly)
                //  (o as DependencyObject).ClearValue(current.Property);
            }

            var lve2 = new List<LocalValueEntry>();
            var localValueEnumerator2 = o2.GetLocalValueEnumerator();
            while (localValueEnumerator2.MoveNext())
            {
                var current = localValueEnumerator2.Current;
                lve2.Add(current);
                //if (!current.Property.ReadOnly)
                //  (o as DependencyObject).ClearValue(current.Property);
            }

            // var d1 = DependencyPropertyDescriptor.FromProperty(lve2[8].Property.);
            return null;
        }
    }
}
