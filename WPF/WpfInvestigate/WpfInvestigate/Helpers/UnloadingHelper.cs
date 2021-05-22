using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

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
            {
                fe.Dispatcher.BeginInvoke(new Action(() => UnloadElement(fe)), DispatcherPriority.Normal); // ? Closing is faster
                UnloadElement(fe);
            }

            return true;
        }

        // public static Dictionary<Type, int> RD = new Dictionary<Type, int>();
        public static void ClearResources(ResourceDictionary rd)
        {
            foreach (var child in rd.MergedDictionaries)
                ClearResources(child);

            foreach (var value in rd.Values)
            {
                if (value is DependencyObject d)
                {
                    EventHelper.RemoveWpfEventHandlers(d); // ???
                    // ClearElement(d);
                }

                if (value is IDisposable disposable)
                    disposable.Dispose();
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
        private static void UnloadElement(DependencyObject d)
        {
            // Debug.Print($"UnloadElement: {a1}");
            foreach (var o in GetVChildren(d).ToArray())
            {
                // BindingOperations.ClearAllBindings(o);
                // BindingHelper.ClearAllBindingsOfObject(o, true);
                EventHelper.RemoveWpfEventHandlers(o);

                ClearElement(o);

                if (o is UIElement uiElement && VisualTreeHelper.GetParent(uiElement) is Panel panel && !panel.IsItemsHost)
                  panel.Children.Remove(uiElement);
                /*if (o is UIElement uiElement)
                {
                    if (VisualTreeHelper.GetParent(uiElement) is DependencyObject _do)
                        RemoveChild(_do, uiElement); // !!! Important
                }*/

                /*if (uiElement != null && uiElement.CommandBindings.Count > 0)
                {
                    //foreach (CommandBinding cb in uiElement.CommandBindings)
                      //  EventHelper.RemoveWpfEventHandlers(cb);
                    uiElement.CommandBindings.Clear();
                }*/

                if (o is FrameworkElement fe)
                {
                    // ClearResources(fe.Resources);
                    fe.Resources.Add("Unloaded", null);
                }
            }
        }

        private static IEnumerable<DependencyObject> GetVChildren(DependencyObject current)
        {
            if (current is Visual || current is Visual3D)
            {
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(current); i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    foreach (var childOfChild in GetVChildren(child))
                        yield return childOfChild;
                }
            }
            yield return current;
        }

        private static void ClearElement(DependencyObject element)
        {
            if (element.IsSealed) return;

            foreach (var pi in GetPropertiesForCleaner(element.GetType()).Where(pi => !pi.PropertyType.IsValueType))
            {
                var value = pi.GetValue(element);

                if (value is ResourceDictionary rd)
                    ClearResources(rd);

                 // if (value != null)
//                    if (value is ICommand || value is INotifyPropertyChanged || value is INotifyCollectionChanged)
                //   EventHelper.RemoveWpfEventHandlers(value);

                if (value != null && pi.CanWrite && (value is ICommand || value is ControlTemplate ||
                                                     value is Style || value is ResourceDictionary ||
                                                     pi.Name == "DataContext" || pi.Name == "Content" ||
                                                     pi.Name == "Command" || pi.Name == "CommandTarget" || pi.Name == "ItemsPanel"))
                    pi.SetValue(element, null);

                //if (value != null && pi.CanWrite)
                  //  pi.SetValue(element, null);

                // if (pi.Name != "TemplatedParent" && pi.Name != "Parent" && pi.Name != "Content" && pi.Name != "DataContext" && (value is ICommand || value is INotifyPropertyChanged || value is INotifyCollectionChanged))
                //  EventHelper.RemoveWpfEventHandlers(value);

                //if (value != null)
                  //  EventHelper.RemoveWpfEventHandlers(value);

                /*if (value is IItemContainerGenerator itemContainerGenerator)
                {
                     EventHelper.RemoveWpfEventHandlers(value);
                     itemContainerGenerator.RemoveAll();
                }*/

                /*if (value is ItemContainerGenerator icg)
                {
                    // var mi = typeof(ItemContainerGenerator).GetMethod("RemoveAllInternal", BindingFlags.Instance | BindingFlags.NonPublic);
                    // mi.Invoke(value, new object[] { true });
                    var fi = typeof(ItemContainerGenerator).GetField("_generator", BindingFlags.Instance | BindingFlags.NonPublic);
                    var generator = (IDisposable) fi.GetValue(value);
                    generator?.Dispose();
                }*/

                // no items: if (value is IDisposable disposable && !(value is Cursor))
                //  disposable.Dispose();

            }
        }

        private static PropertyInfo[] GetPropertiesForCleaner(Type type) => type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        #endregion

        private static void RemoveChildFull(DependencyObject parent, UIElement child)
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

        private static void ClearElementFull(DependencyObject element)
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
                    var value = pi.GetValue(element);
                    if (!(pi.Name == "Language" || pi.Name == "Title") && value != null)
                        pi.SetValue(element, null);

                    if (value != null)
                        EventHelper.RemoveWpfEventHandlers(value);

                    if (value is IList list)
                        list.Clear();
                }
            });
            // errors in Wpf control logic: GetFieldInfoForCleaner(type).ForEach(fieldInfo => { fieldInfo.SetValue(element, null); });
        }

    }
}
