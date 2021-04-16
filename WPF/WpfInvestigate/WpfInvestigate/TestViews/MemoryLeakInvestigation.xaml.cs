using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using WpfInvestigate.Common;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for MemoryLeakInvestigation.xaml
    /// </summary>
    public partial class MemoryLeakInvestigation : Window
    {
        public MemoryLeakInvestigation()
        {
            InitializeComponent();
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            // var a1 = System.Windows.Data.BindingOperations.GetBinding(Button, ContentProperty);
            // BindingOperations.ClearAllBindings(Button);
            // BindingOperations.ClearAllBindings(MwiBootstrapColorTests.Instance);
            // Tips.ClearAllBindings(Button);
            Tips.ClearAllBindings(MwiBootstrapColorTests.Instance);
        }

        private static Dictionary<Type, List<FieldInfo>> _dpOfTypeCache = new Dictionary<Type, List<FieldInfo>>();
        private static int _clearCount = 0;
        public static void ClearAllBindings(DependencyObject target)
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
                        Debug.Print($"Child: {child}, {dp1.Name}, {a1}");
                    }
                });
            }
        }

    }
}
