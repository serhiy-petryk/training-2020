using System.Windows.Media;
using WpfInvestigate.Themes;

namespace WpfInvestigate.Helpers
{
    public interface IColorThemeSupport
    {
        MwiThemeInfo Theme { get; set; }
        Color? ThemeColor { get; set; }
        MwiThemeInfo ActualTheme { get; }
        Color ActualThemeColor { get; }
        void UpdateColorTheme(bool colorChanged, bool processChildren);
    }
}
