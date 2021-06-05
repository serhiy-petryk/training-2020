using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using WpfInvestigate.Common;
using WpfInvestigate.Controls;
using WpfInvestigate.Themes;

namespace WpfInvestigate.Helpers
{
    public interface IColorThemeSupport
    {
        MwiThemeInfo Theme { get; set; }
        Color? ThemeColor { get; set; }
        MwiThemeInfo ActualTheme { get; }
        Color ActualThemeColor { get; }
        IColorThemeSupport ColorThemeParent { get; }
    }

    public static class ColorThemeSupportHelper
    {
        public static MwiThemeInfo GetActualTheme(this IColorThemeSupport obj)
        {
            if (obj.Theme != null) return obj.Theme;
            return obj.ColorThemeParent?.ActualTheme ?? MwiThemeInfo.DefaultTheme;
        }

        public static Color GetActualThemeColor(this IColorThemeSupport obj)
        {
            if (obj.ActualTheme.FixedColor != null) return obj.ActualTheme.FixedColor.Value;
            if (obj.ThemeColor.HasValue) return obj.ThemeColor.Value;
            return obj.ColorThemeParent?.ActualThemeColor ?? MwiThemeInfo.DefaultThemeColor;
        }

        public static void SelectTheme(this IColorThemeSupport obj, FrameworkElement host)
        {
            var d = obj as DependencyObject;

            var defaultTheme = obj.ActualTheme;
            if (obj.Theme != null)
            {
                var a1 = d.GetVisualParents().OfType<IColorThemeSupport>().FirstOrDefault(a => !Equals(a, obj) && a.Theme != null);
                defaultTheme = a1?.Theme ?? MwiThemeInfo.DefaultTheme;
            }

            var defaultThemeColor = obj.ActualThemeColor;
            if (obj.ThemeColor != null)
            {
                var a1 = d.GetVisualParents().OfType<IColorThemeSupport>().FirstOrDefault(a => !Equals(a, obj) && a.ThemeColor != null);
                defaultThemeColor = a1?.ThemeColor ?? MwiThemeInfo.DefaultThemeColor;
            }

            var adorner = new DialogAdorner(host) { CloseOnClickBackground = true };
            var themeSelector = new ThemeSelector
            {
                Margin = new Thickness(0),
                Theme = obj.Theme,
                DefaultTheme = defaultTheme,
                ThemeColor = obj.ThemeColor,
                DefaultThemeColor = defaultThemeColor
            };
            var mwiChild = new MwiChild
            {
                Content = themeSelector,
                Width = 900,
                Height = 600,
                MinWidth = 700,
                MinHeight = 500,
                LimitPositionToPanelBounds = false,
                Title = "Theme Selector",
                VisibleButtons = MwiChild.Buttons.Close | MwiChild.Buttons.Maximize,
            };
            mwiChild.SetBinding(MwiChild.ThemeProperty, new Binding("ActualTheme") { Source = themeSelector });
            mwiChild.SetBinding(MwiChild.ThemeColorProperty, new Binding("ActualThemeColor") { Source = themeSelector, Converter = ColorHslBrush.Instance });
            adorner.ShowContentDialog(mwiChild);

            if (themeSelector.IsSaved)
            {
                obj.Theme = themeSelector.Theme;
                if (obj.ActualTheme.FixedColor.HasValue)
                {
                    if (obj is Control cntrl)
                        cntrl.Background = null;
                }
                else
                    obj.ThemeColor = themeSelector.ThemeColor;
            }
        }
    }
}
