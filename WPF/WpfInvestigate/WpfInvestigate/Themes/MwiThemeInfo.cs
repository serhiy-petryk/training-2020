using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using WpfInvestigate.Common.ColorSpaces;

namespace WpfInvestigate.Themes
{
    public class MwiThemeInfo
    {
        private static string _currentAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        public static Dictionary<string, MwiThemeInfo> Themes = new Dictionary<string, MwiThemeInfo>
        {
            {"Windows7", new MwiThemeInfo("Windows 7", ColorUtils.StringToColor("#FFBBD2EB"),null, new[] { $"pack://application:,,,/{_currentAssemblyName};component/Themes/Mwi.Wnd7.xaml" })},
            {"Windows10", new MwiThemeInfo( "Windows 10", null, null, new[] { $"pack://application:,,,/{_currentAssemblyName};component/Themes/Mwi.Wnd10.xaml"})},
            {"Windows10-2", new MwiThemeInfo( "Windows10 with borders", null,null, new[] { $"pack://application:,,,/{_currentAssemblyName};component/Themes/Mwi.Wnd10.WithBorders.xaml"})}
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
        public string Id => Themes.Where(kvp => kvp.Value == this).Select(kvp => kvp.Key).FirstOrDefault();
        public string Name { get; }
        public Color? FixedColor { get; }
        private string _assemblyName;
        private string[] _uris;
        public MwiThemeInfo(string name, Color? fixedColor, string assemblyName, string[] uris)
        {
            // Id = id;
            Name = name;
            FixedColor = fixedColor;
            _assemblyName = assemblyName;
            _uris = uris;
        }

        public ResourceDictionary[] GetResources()
        {
            if (!string.IsNullOrEmpty(_assemblyName))
            {
                var assembly = GetAssembly(_assemblyName);
                var uris = _uris.Select(uri => new Uri("/" + assembly.GetName().Name + ";component/" + uri, UriKind.Relative));
                return uris.Select(uri => Application.LoadComponent(uri) as ResourceDictionary).ToArray();
            }

            if (_uris != null)
                return _uris.Select(uri => new ResourceDictionary { Source = new Uri(uri, UriKind.RelativeOrAbsolute) }).ToArray();
            return new ResourceDictionary[0];
        }

        public override string ToString() => Id;
    }
}
