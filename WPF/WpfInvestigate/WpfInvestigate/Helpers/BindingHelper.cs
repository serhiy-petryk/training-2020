using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using WpfInvestigate.Common;

namespace WpfInvestigate.Helpers
{
    public static class BindingHelper
    {
        public static bool IsElementDisposing(this FrameworkElement fe)
        {
            var wnd = Window.GetWindow(fe);
            return !fe.IsLoaded && !fe.IsVisible && (fe.Parent == null || (wnd != null && !wnd.IsLoaded && !wnd.IsVisible));
        }

        private static Dictionary<Type, List<FieldInfo>> _dpOfTypeCache = new Dictionary<Type, List<FieldInfo>>();
        public static void UpdateAllBindings(this DependencyObject target)
        // based on 'H.B.' comment in https://stackoverflow.com/questions/5023025/is-there-a-way-to-get-all-bindingexpression-objects-for-a-window
        {
            foreach (var child in (new[] { target }).Union(Tips.GetVisualChildren(target)))
            {
                var type = child.GetType();
                if (!_dpOfTypeCache.ContainsKey(type))
                {
                    var propertiesDp = new List<FieldInfo>();
                    var currentType = type;
                    while (currentType != typeof(object))
                    {
                        propertiesDp.AddRange(currentType.GetFields().Where(x => x.FieldType == typeof(DependencyProperty)));
                        currentType = currentType.BaseType;
                    }
                    _dpOfTypeCache[type] = propertiesDp;
                }

                _dpOfTypeCache[type].ForEach(dp => BindingOperations.GetBindingExpression(child, dp.GetValue(child) as DependencyProperty)?.UpdateTarget());
            }
        }
        private static int _clearCount = 0;
        public static void ClearAllBindings(this DependencyObject target)
        // based on 'H.B.' comment in https://stackoverflow.com/questions/5023025/is-there-a-way-to-get-all-bindingexpression-objects-for-a-window
        {
            foreach (var child in (new[] { target }).Union(Tips.GetVisualChildren(target)))
            {
                var type = child.GetType();
                if (!_dpOfTypeCache.ContainsKey(type))
                {
                    var propertiesDp = new List<FieldInfo>();
                    var currentType = type;
                    while (currentType != typeof(object))
                    {
                        propertiesDp.AddRange(currentType.GetFields().Where(x => x.FieldType == typeof(DependencyProperty)));
                        currentType = currentType.BaseType;
                    }
                    _dpOfTypeCache[type] = propertiesDp;
                }

                _dpOfTypeCache[type].ForEach(dp =>
                {
                    var dp1 = dp.GetValue(child) as DependencyProperty;
                    var a1 = BindingOperations.GetBinding(child, dp1);
                    if (a1 != null)
                    {
                        BindingOperations.ClearBinding(child, dp1);
                        var a2 = BindingOperations.GetBinding(child, dp1);
                        if (a2 != null)
                        {
                            // child.SetValue(dp1, child.GetValue(dp1));
                            child.SetValue(dp1, GetDefaultValue(dp1.PropertyType));
                            Debug.Print(
                                $"ClearBinding: {_clearCount++}, {child}, {(child is FrameworkElement ? ((FrameworkElement)child).Name : null)}, {dp1.Name}, {a1.Path.Path}, {child.GetValue(dp1)}");
                        }
                    }
                });
            }
        }

        private static object GetDefaultValue(Type t) => t.IsValueType ? Activator.CreateInstance(t) : null;
    }
}
