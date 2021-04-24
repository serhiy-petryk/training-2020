using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using WpfInvestigate.Common;

namespace WpfInvestigate.Helpers
{
    public interface IAutomaticUnloading
    {
        void OnUnloaded(object sender, RoutedEventArgs e);
    }

    public static class UnloadingHelper
    {
        public static bool AutomaticUnloading(this IAutomaticUnloading item)
        {
            var fe = item as FrameworkElement;
            if (!fe.IsElementDisposing()) return false;

            fe.Unloaded -=  item.OnUnloaded;
            CleanDependencyObject(fe);
            return true;
        }

        private static bool IsElementDisposing(this FrameworkElement fe)
        {
            var wnd = Window.GetWindow(fe);
            return !fe.IsLoaded && !fe.IsVisible && (wnd == null || fe.Parent == null || (wnd != null && !wnd.IsLoaded && !wnd.IsVisible));
        }

        #region ===========  Dependency Object cleaner  ===============
        private static Dictionary<Type, List<FieldInfo>> _fiCache = new Dictionary<Type, List<FieldInfo>>();
        private static Dictionary<Type, List<PropertyInfo>> _piCache = new Dictionary<Type, List<PropertyInfo>>();

        // todo: add collections & recursive objects (see resources)
        private static void CleanDependencyObject(DependencyObject d)
        {
            // todo: add collections & recursive objects (see resources)
            //var elements = (new[] { d }).Union(d.GetVisualChildren()).ToArray();
            var elements = d is Visual || d is Visual3D ? GetVChildren(d).Union(new[] { d }).ToArray() : new [] {d};

            foreach (var element in elements)
            {
                BindingOperations.ClearAllBindings(element);

                GetPropertiesForCleaner(element.GetType()).ForEach(pi =>
                {
                    var value = pi.GetValue(element);
                    if (value is DependencyObject d1)
                    {
                        BindingOperations.ClearAllBindings(d1);
                        EventHelper.RemoveWpfEventHandlers(d1);
                        Events.RemoveAllEventSubsriptions(d1);
                    }
                    else if (value is ResourceDictionary rd)
                        ClearResources(rd);
                });

                EventHelper.RemoveWpfEventHandlers(element);
                Events.RemoveAllEventSubsriptions(element);

                ClearElement(element); // !! Very important

                if (element is UIElement uIElement)
                {
                    uIElement.CommandBindings.Clear();
                    uIElement.InputBindings.Clear();
                    if (element is FrameworkElement fe2)
                        fe2.Triggers.Clear();

                    if (VisualTreeHelper.GetParent(uIElement) is DependencyObject _do)
                        RemoveChild(_do, uIElement); // !!! Important
                }
            }

            /*foreach (var element in elements)
            {
                EventHelper.RemoveWpfEventHandlers(element);
                Events.RemoveAllEventSubsriptions(element);
            }*/
        }

        private static void RemoveChild(DependencyObject parent, UIElement child)
        {
            if (parent is Panel panel)
            {
                if (panel.IsItemsHost)
                {
                    // Debug.Print($"RemoveChild. Panel. IsItemsHost");
                }
                else
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
            if (parent is ItemsPresenter itemsPresenter)
            {
                return;
            }
            if (parent is ContainerVisual container)
            {
                container.Children.Remove(child);
                return;
            }
            if (parent is Menu menu)
            {
                if (menu.Items.Contains(child))
                    menu.Items.Remove(child);
                return;
            }
            if (parent is MenuItem menuItem)
            {
                if (menuItem.Items.Contains(child))
                    menuItem.Items.Remove(child);
                return;
            }
            if (parent is Slider || parent is Separator)
            {
                return;
            }
            if (parent.GetType().Namespace == "System.Windows.Controls.Primitives")
            {
                return;
            }

            Debug.Print($"RemoveChild not defined for : {parent.GetType()} type of parent");
            return;
            throw new Exception($"RemoveChildis not defined for {parent.GetType().Name} type of parent");
        }

        private static void ClearResources(ResourceDictionary rd)
        {
            /*if (rd.MergedDictionaries.Count != 0)
                Debug.Print($"Merged: {rd.MergedDictionaries.Count}");
            if (rd.Count > 0)
                Debug.Print($"RD: {rd.Count}");*/

            foreach (var child in rd.MergedDictionaries)
                ClearResources(child);

            foreach (var item in rd.OfType<DictionaryEntry>())
            {
                if (item.Value is DependencyObject d)
                    BindingOperations.ClearAllBindings(d);
            }
            rd.Clear();
        }

        private static void ClearElement(DependencyObject element)
        {
            if (element is Track || (element is Freezable freezable && freezable.IsFrozen)) return;

            GetPropertiesForCleaner(element.GetType()).Where(pi => !pi.PropertyType.IsValueType && pi.PropertyType != typeof(FontFamily)).ToList().ForEach(pi =>
            {
                // no effect: if (!(pi.PropertyType == typeof(string)))
                if (!(pi.PropertyType == typeof(string) && string.IsNullOrEmpty((string)pi.GetValue(element))))
                {
                    if (!(pi.Name == "Language" || pi.Name == "Title") && pi.GetValue(element) != null)
                        pi.SetValue(element, null);
                }
            });
            // errors in Wpf control logic: GetFieldInfoForCleaner(type).ForEach(fieldInfo => { fieldInfo.SetValue(element, null); });
        }

        private static List<PropertyInfo> GetPropertiesForCleaner(Type type)
        {
            if (!_piCache.ContainsKey(type))
            {
                var propertyInfos = new List<PropertyInfo>();
                var currentType = type;
                while (currentType != typeof(object))
                {
                    propertyInfos.AddRange(currentType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanWrite));
                    currentType = currentType.BaseType;
                }
                _piCache[type] = propertyInfos;
            }

            return _piCache[type];
        }

        private static List<FieldInfo> GetFieldInfoForCleaner(Type type)
        {
            if (!_fiCache.ContainsKey(type))
            {
                var fieldInfos = new List<FieldInfo>();
                var currentType = type;
                while (currentType != typeof(object))
                {
                    fieldInfos.AddRange(currentType.GetFields().Where(x => !x.IsStatic && !x.IsInitOnly && x.IsPublic && !x.FieldType.IsValueType));
                    currentType = currentType.BaseType;
                }

                _fiCache[type] = fieldInfos;
            }

            return _fiCache[type];
        }
        #endregion

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
