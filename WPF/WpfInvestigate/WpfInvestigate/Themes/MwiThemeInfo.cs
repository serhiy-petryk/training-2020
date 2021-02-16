using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace WpfInvestigate.Themes
{
    public class MwiThemeInfo
    {
        public static MwiThemeInfo[] Themes =
        {
            new MwiThemeInfo("Windows10", "Стиль Windows 10", null, new[] { "/Themes/Wnd7.xaml"}),
            new MwiThemeInfo("Windows7", "Стиль Windows 7", null, new[] { "/Themes/Wnd10.xaml"})
            /*new MwiThemeInfo("Windows10", "Стиль Windows 10", null, new[] { "/Mwi/Themes/MwiChild.Wnd10.xaml", "/Themes/MwiContainer.Wnd7.xaml"}),
            new MwiThemeInfo("Windows7", "Стиль Windows 7", null, new[] { "/Mwi/Themes/MwiChild.Wnd7.xaml", "/Themes/MwiContainer.Wnd7.xaml"})*/
        };

        // ==============   Static section  ======================
        private static Dictionary<string, Assembly> _assemblyCache = new Dictionary<string, Assembly>();
        // Luna/Luna homestead/Royale are very similar themes

        private static Assembly GetAssembly(string assemblyName)
        {
            var curAsm = Assembly.GetExecutingAssembly();
            if (!_assemblyCache.ContainsKey(assemblyName))
            {
                var resource = assemblyName;
                using (var stm = curAsm.GetManifestResourceStream(resource))
                {
                    var ba = new byte[(int)stm.Length];
                    stm.Read(ba, 0, (int)stm.Length);
                    _assemblyCache.Add(assemblyName, Assembly.Load(ba));
                }
            }
            return _assemblyCache[assemblyName];
        }

        // ==============   Instance section  ======================
        public string Id { get; }
        public string Name { get; }

        private string _assemblyName;
        private string[] _uris;
        private ResourceDictionary[] _resources = null;
        public MwiThemeInfo(string id, string name, string assemblyName, string[] uris)
        {
            Id = id;
            Name = name;
            _assemblyName = assemblyName;
            _uris = uris;
        }

        public void ApplyTheme()
        {
            if (!string.IsNullOrEmpty(_assemblyName) && _resources == null)
            {
                var assembly = GetAssembly(_assemblyName);
                var uris = _uris.Select(uri => new Uri("/" + assembly.GetName().Name + ";component/" + uri, UriKind.Relative));
                _resources = uris.Select(uri => Application.LoadComponent(uri) as ResourceDictionary).ToArray();
            }
            else if (_resources == null && _uris != null)
                _resources = _uris.Select(uri => new ResourceDictionary { Source = new Uri(uri, UriKind.RelativeOrAbsolute) }).ToArray();
            else if (_resources == null && _uris == null)
                _resources = new ResourceDictionary[0];

            // Clear old themes and add new theme
            Application.Current.Resources.MergedDictionaries.Clear();
            foreach (var r in _resources)
                Application.Current.Resources.MergedDictionaries.Add(r);
        }

        public override string ToString() => Id;
    }
}
