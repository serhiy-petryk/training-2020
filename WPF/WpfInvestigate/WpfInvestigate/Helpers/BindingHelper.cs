using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using WpfInvestigate.Common;

namespace WpfInvestigate.Helpers
{
    public static class BindingHelper
    {
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
            var aa = target is Visual || target is Visual3D ? (new[] { target }).Union(target.GetVisualChildren()) : new[] { target };
            foreach (var child in aa)
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
