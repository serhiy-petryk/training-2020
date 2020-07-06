using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace IconViewer.Themes
{
  public class ThemeInfo
  {
    // ==============   Static section  ======================
    private static Dictionary<string, Assembly> _assemblyCache = new Dictionary<string, Assembly>();
    // Luna/Luna homestead/Royale are very similar themes
    public static ThemeInfo[] Themes = new[]
    {
      new ThemeInfo("Default", null, null),
      new ThemeInfo("Aero", "IconViewer.Themes.PresentationFramework.Aero.dll", "themes/aero.normalcolor.xaml"),
      // bad assembly new ThemeInfo("Aero2", "IconViewer.Themes.PresentationFramework.Aero2.dll", "themes/aero2.normalcolor.xaml"),
      new ThemeInfo("Aero Lite", "IconViewer.Themes.PresentationFramework.AeroLite.dll",
        "themes/aerolite.normalcolor.xaml"),
      new ThemeInfo("Classic", "IconViewer.Themes.PresentationFramework.Classic.dll", "themes/classic.xaml"),
      new ThemeInfo("Luna", "IconViewer.Themes.PresentationFramework.Luna.dll", "themes/luna.normalcolor.xaml"),
      new ThemeInfo("Luna metallic", "IconViewer.Themes.PresentationFramework.Luna.dll", "themes/luna.metallic.xaml"),
      new ThemeInfo("Luna homestead", "IconViewer.Themes.PresentationFramework.Luna.dll", "themes/luna.homestead.xaml"),
      new ThemeInfo("Royale", "IconViewer.Themes.PresentationFramework.Royale.dll", "themes/royale.normalcolor.xaml")
    };

    private static Assembly GetAssembly(string assemblyName)
    {
      Assembly curAsm = Assembly.GetExecutingAssembly();
      if (!_assemblyCache.ContainsKey(assemblyName))
      {
        var resource = assemblyName;
        using (var stm = curAsm.GetManifestResourceStream(resource))
        {
          var ba = new byte[(int) stm.Length];
          stm.Read(ba, 0, (int) stm.Length);
          _assemblyCache.Add(assemblyName, Assembly.Load(ba));
        }
      }
      return _assemblyCache[assemblyName];
    }

    // ==============   Instance section  ======================
    public string Name;
    public string AssemblyName;
    public string ComponentName;
    public ResourceDictionary Resource { get; private set; }
    public Brush BackgroundColor { get; private set; }

    public ThemeInfo(string name, string assemblyName, string componentName)
    {
      Name = name;
      AssemblyName = assemblyName;
      ComponentName = componentName;

      if (!string.IsNullOrEmpty(AssemblyName))
      {
        var assembly = GetAssembly(AssemblyName);
        var uri = new Uri("/" + assembly.GetName().Name + ";component/" + ComponentName, UriKind.Relative);
        Resource = Application.LoadComponent(uri) as ResourceDictionary;
        var style = Resource[typeof(ToolBar)] as Style;
        var setter = style.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == Control.BackgroundProperty);
        if (setter != null)
          BackgroundColor = setter.Value as Brush;
      }

    }

    public override string ToString() => Name;
  }
}
