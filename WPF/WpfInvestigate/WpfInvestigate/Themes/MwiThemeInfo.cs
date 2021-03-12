using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace WpfInvestigate.Themes
{
    public class MwiThemeInfo
    {
        private static string _currentAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        public static MwiThemeInfo[] Themes =
        {
            new MwiThemeInfo("Windows10 (internal resize thumbs)", null, new[] { $"pack://application:,,,/{_currentAssemblyName};component/Themes/Mwi.Wnd10.InternalResizeThumbs.xaml"}),
            new MwiThemeInfo("Windows10", null, new[] { $"pack://application:,,,/{_currentAssemblyName};component/Themes/Mwi.Wnd10.xaml"}),
            new MwiThemeInfo("Windows7", null, new[] { $"pack://application:,,,/{_currentAssemblyName};component/Themes/Mwi.Wnd7.xaml" })
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
        private string _assemblyName;
        private string[] _uris;
        private ResourceDictionary[] _resources = null;
        public MwiThemeInfo(string id, string assemblyName, string[] uris)
        {
            Id = id;
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
                _resources = _uris.Select(uri => new ResourceDictionary{Source = new Uri(uri, UriKind.RelativeOrAbsolute)}).ToArray();
            else if (_resources == null && _uris == null)
                _resources = new ResourceDictionary[0];

            // Clear old themes and add new theme
            // Application.Current.Resources.MergedDictionaries.Clear();
            foreach (var rd in Application.Current.Resources.MergedDictionaries.Where(d => d.Source.OriginalString.Contains("Mwi")).ToArray())
                Application.Current.Resources.MergedDictionaries.Remove(rd);

            foreach (var r in _resources)
                Application.Current.Resources.MergedDictionaries.Add(r);
        }

        public override string ToString() => Id;
    }
}
