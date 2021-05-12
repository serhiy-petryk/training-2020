﻿using System;
using System.Collections;
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

            foreach (var element in elements)
            {
                BindingOperations.ClearAllBindings(element);
                EventHelper.RemoveWpfEventHandlers(element);

                ClearElement(element);

                if (element is UIElement uiElement && VisualTreeHelper.GetParent(uiElement) is Panel panel && !panel.IsItemsHost)
                    panel.Children.Remove(uiElement);

                if (element is FrameworkElement fe)
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

            GetPropertiesForCleaner(element.GetType()).Where(pi => pi.CanWrite && !pi.PropertyType.IsValueType && pi.PropertyType != typeof(string)).ToList().ForEach(pi =>
            {
                var value = pi.GetValue(element);
                if (value != null && (value is ICommand || value is ControlTemplate || value is Style || value is ResourceDictionary || pi.Name == "DataContext" || pi.Name == "Content" || pi.Name == "Command" || pi.Name == "CommandTarget" || pi.Name == "CommandParameter"))
                    pi.SetValue(element, null);
            });
        }

        private static PropertyInfo[] GetPropertiesForCleaner(Type type) => type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        #endregion
    }
}
