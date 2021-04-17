﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using WpfInvestigate.Common;

namespace WpfInvestigate.Helpers
{
    public static class CleanerHelper
    {
        public static bool IsElementDisposing(this FrameworkElement fe)
        {
            var wnd = Window.GetWindow(fe);
            return !fe.IsLoaded && !fe.IsVisible && (fe.Parent == null || (wnd != null && !wnd.IsLoaded && !wnd.IsVisible));
        }

        #region ===========  Dependency Object cleaner  ===============
        private static Dictionary<Type, List<FieldInfo>> _fiCache = new Dictionary<Type, List<FieldInfo>>();
        private static Dictionary<Type, List<PropertyInfo>> _piCache = new Dictionary<Type, List<PropertyInfo>>();

        // todo: add collections & recursive objects (see resources)
        public static void CleanDependencyObject(this DependencyObject d)
        {
            // todo: add collections & recursive objects (see resources)
            var elements = GetVChildren(d).Union(new[] { d }).ToArray();

            foreach (var element in elements)
                element.ClearAllBindings(true);

            ClearElements(elements);

            foreach (var element in elements.OfType<UIElement>())
            {
                element.CommandBindings.Clear();
                element.InputBindings.Clear();
                if (element is FrameworkElement fe)
                    fe.Triggers.Clear();
            }

            foreach (var element in elements.OfType<UIElement>())
            {
                var p = VisualTreeHelper.GetParent(element);
                if (p != null)
                    RemoveChild(p, element);
            }

            foreach (var element in elements.OfType<FrameworkElement>())
                ClearResources(element.Resources);
        }

        private static void RemoveChild(DependencyObject parent, UIElement child)
        {
            if (parent is Panel panel)
            {
                panel.Children.Remove(child);
                return;
            }
            if (parent is Decorator decorator)
            {
                if (Equals(decorator.Child, child)) decorator.Child = null;
                return;
            }
            if (parent is ContentPresenter contentPresenter)
            {
                if (Equals(contentPresenter.Content, child)) contentPresenter.Content = null;
                return;
            }
            if (parent is ContentControl contentControl)
            {
                if (Equals(contentControl.Content, child)) contentControl.Content = null;
                return;
            }

            throw new Exception($"RemoveChildis not defined for {parent.GetType().Name} type of parent");
        }

        private static void ClearResources(ResourceDictionary rd)
        {
            if (rd.MergedDictionaries.Count != 0)
                Debug.Print($"Merged: {rd.MergedDictionaries.Count}");
            if (rd.Count > 0)
                Debug.Print($"RD: {rd.MergedDictionaries.Count}");

            foreach (var child in rd.MergedDictionaries)
                ClearResources(child);
            rd.Clear();
        }

        private static void ClearElements(DependencyObject[] elements)
        {
            foreach (var element in elements)
            {
                var type = element.GetType();
                if (!_fiCache.ContainsKey(type))
                {
                    var fieldInfos = new List<FieldInfo>();
                    var currentType = type;
                    while (currentType != typeof(object))
                    {
                        fieldInfos.AddRange(currentType.GetFields()
                            .Where(x => !x.IsStatic && !x.IsInitOnly && x.IsPublic && !x.FieldType.IsValueType));
                        currentType = currentType.BaseType;
                    }

                    _fiCache[type] = fieldInfos;
                }

                if (!_piCache.ContainsKey(type)) { 
                    var propertyInfos = new List<PropertyInfo>();
                    var currentType = type;
                    while (currentType != typeof(object))
                    {
                        propertyInfos.AddRange(currentType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(x => x.CanWrite && !x.PropertyType.IsValueType));
                        currentType = currentType.BaseType;
                    }
                    _piCache[type] = propertyInfos;
                }
            }

            foreach (var element in elements)
            {
                var type = element.GetType();
                _fiCache[type].ForEach(fieldInfo => { fieldInfo.SetValue(element, null); });
                _piCache[type].Where(pi => pi.PropertyType != typeof(FontFamily)).ToList().ForEach(pi =>
                {
                    if (!(pi.PropertyType == typeof(string) && string.IsNullOrEmpty((string)pi.GetValue(element))))
                        pi.SetValue(element, null);
                });
            }
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
        #endregion

        private static Dictionary<Type, List<FieldInfo>> _fiOfDpCache = new Dictionary<Type, List<FieldInfo>>();
        public static void UpdateAllBindings(this DependencyObject target)
        // based on 'H.B.' comment in https://stackoverflow.com/questions/5023025/is-there-a-way-to-get-all-bindingexpression-objects-for-a-window
        {
            foreach (var child in (new[] { target }).Union(Tips.GetVisualChildren(target)))
            {
                var fiOfDp = GetFieldInfosOfDp(child.GetType());
                fiOfDp.ForEach(dp => BindingOperations.GetBindingExpression(child, dp.GetValue(child) as DependencyProperty)?.UpdateTarget());
            }
        }
        public static void ClearAllBindings(this DependencyObject target, bool hardMethod = false)
        // based on 'H.B.' comment in https://stackoverflow.com/questions/5023025/is-there-a-way-to-get-all-bindingexpression-objects-for-a-window
        {
            /*foreach (var child in (new[] {target}).Union(Tips.GetVisualChildren(target)))
            {
                BindingOperations.ClearAllBindings(child);
            }

            return;*/

            foreach (var child in (new[] { target }).Union(Tips.GetVisualChildren(target)))
            {
                var fiOfDp = GetFieldInfosOfDp(child.GetType());
                fiOfDp.ForEach(dp =>
                {
                    var dp1 = dp.GetValue(child) as DependencyProperty;
                    var a1 = BindingOperations.GetBinding(child, dp1);
                    if (a1 != null)
                    {
                        // Debug.Print($"ClearBinding: {_clearCount++}, {child}, {(child is FrameworkElement ? ((FrameworkElement)child).Name : null)}, {dp1.Name}, {a1.Path.Path}, {child.GetValue(dp1)}");
                        BindingOperations.ClearBinding(child, dp1);
                        var a2 = BindingOperations.GetBinding(child, dp1);
                        if (a2 != null)
                        {
                            if (!hardMethod)
                                child.SetValue(dp1, child.GetValue(dp1));
                            else
                                child.SetValue(dp1, GetDefaultValue(dp1.PropertyType));
                            // Debug.Print($"ClearBinding: {_clearCount++}, {child}, {(child is FrameworkElement ? ((FrameworkElement)child).Name : null)}, {dp1.Name}, {a1.Path.Path}, {child.GetValue(dp1)}");
                        }
                    }
                });
            }
        }

        private static object GetDefaultValue(Type t) => t.IsValueType ? Activator.CreateInstance(t) : null;

        private static List<FieldInfo> GetFieldInfosOfDp(Type type)
        {
            if (!_fiOfDpCache.ContainsKey(type))
            {
                var propertiesDp = new List<FieldInfo>();
                var currentType = type;
                while (currentType != typeof(object))
                {
                    propertiesDp.AddRange(currentType.GetFields().Where(x => x.FieldType == typeof(DependencyProperty)));
                    currentType = currentType.BaseType;
                }
                _fiOfDpCache[type] = propertiesDp;
            }

            return _fiOfDpCache[type];
        }
    }
}
