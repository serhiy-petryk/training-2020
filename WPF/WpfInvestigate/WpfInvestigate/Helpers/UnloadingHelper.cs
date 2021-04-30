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

namespace WpfInvestigate.Helpers
{
    public interface IAutomaticUnloading
    {
        void OnUnloaded(object sender, RoutedEventArgs e);
    }

    public static class UnloadingHelper
    {
        #region ========  Public section  =========
        public static bool AutomaticUnloading(this FrameworkElement fe)
        {
            if (!fe.IsElementDisposing()) return false;

            if (fe is IAutomaticUnloading autoUnloadingItem)
            {
                fe.Unloaded -= autoUnloadingItem.OnUnloaded;
                if (fe.Resources.Contains("Unloaded"))
                    return true;
            }

            CleanDependencyObject(fe);
            return true;
        }

        public static void ClearResources(ResourceDictionary rd)
        {
            foreach (var child in rd.MergedDictionaries)
                ClearResources(child);

            foreach (var item in rd.OfType<DictionaryEntry>())
            {
                if (item.Value is DependencyObject d && !(d is IAutomaticUnloading))
                {
                    BindingOperations.ClearAllBindings(d);
                    EventHelper.RemoveWpfEventHandlers(d); // ???
                    EventHelperOld.RemoveAllEventSubsriptions(d); // ???
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
        // todo: add collections & recursive objects (see resources)
        private static void CleanDependencyObject(DependencyObject d)
        {
            // todo: add collections & recursive objects (see resources)
            //var elements = (new[] { d }).Union(d.GetVisualChildren()).ToArray();
            var elements = d is Visual || d is Visual3D ? GetVChildren(d).Union(new[] { d }).ToArray() : new [] {d};

            foreach (var element in elements)
            {
                BindingOperations.ClearAllBindings(element);
                EventHelper.RemoveWpfEventHandlers(element);
                EventHelperOld.RemoveAllEventSubsriptions(element);

                foreach (var pi in GetPropertiesForCleaner(element.GetType()))
                {
                    var value = pi.GetValue(element);
                    if (value is DependencyObject d1)
                    {
                        BindingOperations.ClearAllBindings(d1); // !! Important. Remove error on MwiStartup test (Layout transform)
                        EventHelper.RemoveWpfEventHandlers(d1);
                        EventHelperOld.RemoveAllEventSubsriptions(d1);
                    }
                    else if (value is ResourceDictionary rd)
                        ClearResources(rd);
                    /*else if (value is ICollection c)
                    {
                        EventHelper.RemoveWpfEventHandlers(c);
                        EventHelperOld.RemoveAllEventSubsriptions(c);
                    }*/
                }

                ClearElement(element); // !! Very important

                if (element is UIElement uIElement)
                {
                    /*if (uIElement.CommandBindings.Count > 0)
                        uIElement.CommandBindings.Clear();
                    if (uIElement.InputBindings.Count > 0)
                        uIElement.InputBindings.Clear();
                    if (element is FrameworkElement fe2 && fe2.Triggers.Count > 0)
                        fe2.Triggers.Clear();*/

                    if (VisualTreeHelper.GetParent(uIElement) is DependencyObject _do)
                        RemoveChild(_do, uIElement); // !!! Important
                }

                if (element is IAutomaticUnloading && element is FrameworkElement fe)
                    fe.Resources.Add("Unloaded", null);
            }
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
            if (parent is Slider || parent is Separator || parent is TabControl || parent is TextBox)
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

        private static void ClearElement(DependencyObject element)
        {
            if (element is Track || (element is Freezable freezable && freezable.IsFrozen)) return;

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

        private static PropertyInfo[] GetPropertiesForCleaner(Type type) => type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

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
    }
}
