using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
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
                UnloadElement(fe);
            return true;
        }

        public static void ClearResources(ResourceDictionary rd)
        {
            foreach (var child in rd.MergedDictionaries)
                ClearResources(child);

            foreach (var value in rd.Values)
            {
                if (value is IDisposable disposable)
                    disposable.Dispose();

                if (value is DependencyObject d)
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
        private static void UnloadElement(FrameworkElement element)
        {
            foreach (var o in GetVChildren(element).Union(new[] { element }).ToArray())
            {
                BindingOperations.ClearAllBindings(o);
                EventHelper.RemoveWpfEventHandlers(o);

                ClearElement(o);

                if (o is UIElement uiElement && VisualTreeHelper.GetParent(uiElement) is Panel panel && !panel.IsItemsHost)
                    panel.Children.Remove(uiElement);

                /*if (uiElement != null && uiElement.CommandBindings.Count > 0)
                {
                    //foreach (CommandBinding cb in uiElement.CommandBindings)
                      //  EventHelper.RemoveWpfEventHandlers(cb);
                    uiElement.CommandBindings.Clear();
                }*/

                if (o is FrameworkElement fe)
                {
                    ClearResources(fe.Resources);
                    fe.Resources.Add("Unloaded", null);
                }
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

        private static void ClearElement(DependencyObject element)
        {
            if (element.IsSealed) return;

            foreach (var pi in GetPropertiesForCleaner(element.GetType()).Where(pi => !pi.PropertyType.IsValueType && pi.PropertyType != typeof(string)))
            {
                var value = pi.GetValue(element);

                if (value != null && pi.CanWrite && (value is ICommand || value is ControlTemplate ||
                                                     value is Style || value is ResourceDictionary ||
                                                     pi.Name == "DataContext" || pi.Name == "Content" ||
                                                     pi.Name == "Command" || pi.Name == "CommandTarget"))
                    pi.SetValue(element, null);

                //if (value is ICommand || value is INotifyPropertyChanged || value is INotifyCollectionChanged)
                  //  EventHelper.RemoveWpfEventHandlers(value);
            }
        }

        private static PropertyInfo[] GetPropertiesForCleaner(Type type) => type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        #endregion
    }
}
