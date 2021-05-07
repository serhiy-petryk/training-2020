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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WpfInvestigate.Common;
using WpfInvestigate.Controls;
using WpfInvestigate.Themes;

namespace WpfInvestigate.Helpers
{
    public static class UnloadingHelper
    {
        #region ========  Public section  =========
        public static bool AutomaticUnloading(this FrameworkElement fe, RoutedEventHandler onUnloadedEventHandler)
        {
            if (!fe.IsElementDisposing()) return false;

            if (onUnloadedEventHandler != null)
                fe.Unloaded -= onUnloadedEventHandler;
            // if (!fe.Resources.Contains("Unloaded"))
                CleanDependencyObject(fe);
            return true;
        }

        public static void ClearResources(ResourceDictionary rd)
        {
            foreach (var child in rd.MergedDictionaries)
                ClearResources(child);

            foreach (var item in rd.OfType<DictionaryEntry>())
            {
                if (item.Value is IDisposable disposable)
                    disposable.Dispose();

                if (item.Value is DependencyObject d)
                {
                    BindingOperations.ClearAllBindings(d);
                    EventHelper.RemoveWpfEventHandlers(d); // ???
                }

            }
            if (!rd.IsReadOnly)
                rd.Clear();
        }
        public static bool IsElementDisposing(this FrameworkElement fe)
        {
            var wnd = Window.GetWindow(fe);
            return !fe.IsLoaded && !fe.IsVisible && (wnd == null || fe.Parent == null || (wnd != null && !wnd.IsLoaded && !wnd.IsVisible));
        }

        #endregion

        #region ===========  Dependency Object cleaner  ===============

        private static int _itemCount = 0;
        private static int _stepCount = 0;
        // todo: add collections & recursive objects (see resources)
        private static void CleanDependencyObject(UIElement d)
        {
            // TypeHelper.CheckNewProperties();

            // todo: add collections & recursive objects (see resources)
            //var elements = (new[] { d }).Union(d.GetVisualChildren()).ToArray();
            var elements = GetVChildren(d).Union(new[] { d }).ToArray();

            // _itemCount += elements.Length;
            // Debug.Print($"Clean: {d.GetType().Name}, items: {_itemCount}, steps: {++_stepCount}");


            // EventHelper.RemoveDPDEvents(elements);

            foreach (var element in elements)
            {
                BindingOperations.ClearAllBindings(element);
                EventHelper.RemoveWpfEventHandlers(element);

                /*// foreach (var pi in GetPropertiesForCleaner(element.GetType()))
                foreach (var pi in GetPropertiesForCleaner(element.GetType()).Where(a => a.Name != "TemplatedParent" && a.Name != "Parent" && a.Name != "Content" && a.Name != "DataContext"))
                {
                    var value = pi.GetValue(element);
                    if (value is DependencyObject d1)
                    {
                        BindingOperations.ClearAllBindings(d1); // !! Important. Remove error on MwiStartup test (Layout transform)
                        EventHelper.RemoveWpfEventHandlers(d1);
                    }
                    else if (value is ResourceDictionary rd)
                        ClearResources(rd);
                    //else if (value is ICollection c)
                    {
                        // EventHelper.RemoveWpfEventHandlers(c);
                        //EventHelperOld.RemoveAllEventSubsriptions(c);
                    }
                }*/

                ClearElement(element);

                if (element is UIElement uiElement && VisualTreeHelper.GetParent(uiElement) is DependencyObject _do)
                    RemoveChild(_do, uiElement); // !!! Important

                if (element is FrameworkElement fe)
                {
                    ClearResources(fe.Resources);
                    fe.Resources.Add("Unloaded", null);
                }
            }

            // EventHelper.RemoveDPDEvents(elements);

            /*//var sw = new Stopwatch();
            //sw.Start();

            foreach (var element in elements)
                BindingOperations.ClearAllBindings(element);
            //var d1 = sw.ElapsedMilliseconds;
            //sw.Restart();

            foreach (var element in elements)
                EventHelper.RemoveWpfEventHandlers(element);
            //var d2 = sw.ElapsedMilliseconds;
            //sw.Restart();

            foreach (var fe in elements.OfType<FrameworkElement>())
            {
                ClearResources(fe.Resources);
                // fe.Resources.Clear();
            }
            //var d3 = sw.ElapsedMilliseconds;
            //sw.Restart();

            foreach (var uiElement in elements.OfType<UIElement>())
            {
                if (VisualTreeHelper.GetParent(uiElement) is DependencyObject _do)
                    RemoveChild(_do, uiElement); // !!! Important
            }

            foreach (var fe in elements.OfType<FrameworkElement>())
                fe.Resources.Add("Unloaded", null);

            //if (element is FrameworkElement fe)
            //  fe.Resources.Add("Unloaded", null);

            //var d4 = sw.ElapsedMilliseconds;
            //sw.Stop();

            // Debug.Print($"SW: {d1}, {d2}, {d3}, {d4}");*/

            /*return;

            foreach (var element in elements)
            {
                BindingOperations.ClearAllBindings(element);
                EventHelper.RemoveWpfEventHandlers(element);

                foreach (var pi in GetPropertiesForCleaner(element.GetType()))
                {
                    var value = pi.GetValue(element);
                    if (value is DependencyObject d1)
                    {
                        // BindingOperations.ClearAllBindings(d1); // !! Important. Remove error on MwiStartup test (Layout transform)
                        // EventHelper.RemoveWpfEventHandlers(d1);
                    }
                    else if (value is ResourceDictionary rd)
                        ClearResources(rd);
                    //else if (value is ICollection c)
                    {
                       // EventHelper.RemoveWpfEventHandlers(c);
                        //EventHelperOld.RemoveAllEventSubsriptions(c);
                    }
                }

                // ClearElement(element); // !! Very important

                if (element is UIElement uIElement)
                {
                    //if (uIElement.CommandBindings.Count > 0)
                      //  uIElement.CommandBindings.Clear();
                    //if (uIElement.InputBindings.Count > 0)
                      //  uIElement.InputBindings.Clear();
                    //if (element is FrameworkElement fe2 && fe2.Triggers.Count > 0)
                      //  fe2.Triggers.Clear();

                    if (VisualTreeHelper.GetParent(uIElement) is DependencyObject _do)
                        RemoveChild(_do, uIElement); // !!! Important
                }

                //if (element is FrameworkElement fe)
                  //  fe.Resources.Add("Unloaded", null);
            }*/
        }

        private static void RemoveChild(DependencyObject parent, UIElement child)
        {
            if (parent is Panel panel)
            {
                if (!panel.IsItemsHost)
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
            if (parent is Slider || parent is Separator || parent is TabControl || parent is TextBox || parent is ItemsPresenter)
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

        private static PropertyInfo[] GetPropertiesForCleaner(Type type) => type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        private static void ClearElement2(DependencyObject element)
        {

            /*var localValueEnumerator = element.GetLocalValueEnumerator();
            while (localValueEnumerator.MoveNext())
            {
                var current = localValueEnumerator.Current;
                if (!current.Property.ReadOnly)
                    element.ClearValue(current.Property);
            }
            return;*/
            if (element is Track || element.IsSealed) return;
            GetPropertiesForCleaner(element.GetType()).Where(pi => pi.CanWrite && !pi.PropertyType.IsValueType && pi.PropertyType != typeof(FontFamily)).ToList().ForEach(pi =>
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

        public static Dictionary<PropertyInfo, int> AAA = new Dictionary<PropertyInfo, int>();
        private static void ClearElement3(DependencyObject element)
        {
            // if (!(element is MwiChild)) return;
            /*var localValueEnumerator = element.GetLocalValueEnumerator();
            while (localValueEnumerator.MoveNext())
            {
                var current = localValueEnumerator.Current;
                if (!current.Property.ReadOnly && typeof(ICommand).IsAssignableFrom(current.Property.PropertyType))
                {
                    // if (!(current.Property.Name == "Language" || current.Property.Name == "Title") && element.GetValue(current.Property) != null)
                    element.SetValue(current.Property, null);
                    // element.ClearValue(current.Property);
                }
            }
            return;*/
            if (element is Track || element.IsSealed) return;
            GetPropertiesForCleaner(element.GetType()).Where(pi => pi.CanWrite && !pi.PropertyType.IsValueType && pi.PropertyType != typeof(FontFamily)).ToList().ForEach(pi =>
            {
                // no effect: if (!(pi.PropertyType == typeof(string)))
                if (!(pi.PropertyType == typeof(string) && string.IsNullOrEmpty((string)pi.GetValue(element))))
                {
                    if (!(pi.Name == "Language" || pi.Name == "Title") && pi.GetValue(element) != null)
                    {
                        if (typeof(ICommand).IsAssignableFrom(pi.PropertyType))
                        {
                            pi.SetValue(element, null);
                        }
                    }
                }
            });
        }

        private static void ClearElement_ICommand(DependencyObject element)
        {
            if (element.IsSealed) return;

            foreach (var pi in GetPropertiesForCleaner(element.GetType()))
                if (typeof(ICommand).IsAssignableFrom(pi.PropertyType))
                    pi.SetValue(element, null);
        }

        private static void ClearElement_DataContext(DependencyObject element)
        {
            if (element.IsSealed) return;

            if (element.GetType().GetProperty("DataContext") is PropertyInfo pi)
                pi.SetValue(element, null);
        }

        private static void ClearElement_Enumerator(DependencyObject element)
        {
            if (element.IsSealed) return;

            var localValueEnumerator = element.GetLocalValueEnumerator();
            while (localValueEnumerator.MoveNext())
            {
                var current = localValueEnumerator.Current;
                if (!current.Property.ReadOnly)
                    element.ClearValue(current.Property);
                // if (current.Value is DependencyObject d)
                //  d.ClearAllBindings();
            }
        }

        private static void ClearElement_Test(DependencyObject element)
        {
            // if (element is Track || element.IsSealed) return;
            if (element.IsSealed) return;

            foreach (var pi in GetPropertiesForCleaner(element.GetType()).Where(pi => pi.CanWrite && !pi.PropertyType.IsValueType && pi.PropertyType != typeof(string)))
                if (pi.GetValue(element) is object o)
                {
                    if (o is ICommand) // 26 new weakrefs
                        pi.SetValue(element, null);
                    if (o is Animatable) // 2 new weakrefs
                        pi.SetValue(element, null);
                    else if (o is ControlTemplate) // 3 new weakrefs
                        pi.SetValue(element, null);
                    //else if (o is Style)
                    //  pi.SetValue(element, null);
                    //else if (pi.Name == "Content" || pi.Name == "DataContext" || pi.Name == "ToolTip")
                    //  pi.SetValue(element, null);
                    /*else if (o is FrameworkElement)
                        pi.SetValue(element, null);
                    else if (o is UIElement)
                        pi.SetValue(element, null);
                    else if (o is ResourceDictionary)
                        pi.SetValue(element, null);
                    else if (o is IEnumerable)
                        pi.SetValue(element, null);*/
                    else if (o is MwiThemeInfo)
                        pi.SetValue(element, null);
                    else
                    {
                        if (!AAA.ContainsKey(pi))
                            AAA.Add(pi, 0);
                        AAA[pi]++;
                        // pi.SetValue(element, null);
                    }

                }
        }

        private static void ClearElement(DependencyObject element)
        {
            if (element.IsSealed) return;

            // ClearElement_DataContext(element);
            // ClearElement_Enumerator(element);  // 11 new weakrefs
            ClearElement_Test(element); // 3 new weakrefs
        }

    }
}
