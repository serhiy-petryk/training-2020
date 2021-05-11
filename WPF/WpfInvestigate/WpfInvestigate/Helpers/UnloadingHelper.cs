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
            if (!fe.Resources.Contains("Unloaded"))
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

                //if (element is UIElement uiElement && VisualTreeHelper.GetParent(uiElement) is DependencyObject o)
                  //  RemoveChild(o, uiElement); // !!! Important
                if (element is UIElement uiElement && VisualTreeHelper.GetParent(uiElement) is Panel panel && !panel.IsItemsHost)
                    panel.Children.Remove(uiElement);

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
            /* if (parent is Decorator decorator)
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

            Debug.Print($"RemoveChild not defined for : {parent.GetType()} type of parent");*/
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

        private static void ClearElement(DependencyObject element)
        {
            if (element.IsSealed) return;

            //if (element is Panel panel && !panel.IsItemsHost && panel.Children.Count != 0)
               // panel.Children.Clear();

            foreach (var pi in element.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Where(pi => pi.CanWrite && !pi.PropertyType.IsValueType && pi.PropertyType != typeof(string)))
            {
                var value = pi.GetValue(element);
                if (value != null && (value is ICommand || value is ControlTemplate || value is Style || value is ResourceDictionary || pi.Name == "DataContext" || pi.Name == "Content" || pi.Name == "Command" || pi.Name == "CommandTarget" || pi.Name == "CommandParameter"))
                    pi.SetValue(element, null);
            }
        }

    }
}
