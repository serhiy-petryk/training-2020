using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfInvestigate.Helpers
{
    public static class DependencyObjectHelper
    {
        private static Dictionary<Type, DependencyProperty[]> _dpOfType = new Dictionary<Type, DependencyProperty[]>();

        public static DependencyProperty[] GetDependencyPropertiesForType(Type type)
        {
            if (!_dpOfType.ContainsKey(type))
                _dpOfType[type] = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(f => f.FieldType == typeof(DependencyProperty))
                    .Select(fieldInfo => fieldInfo.GetValue(null) as DependencyProperty).ToArray();
            return _dpOfType[type];
        }
    }
}
