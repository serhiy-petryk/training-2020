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
        void UpdateColorTheme(bool colorChanged, bool processChildren);
    }

    public static class ColorThemeSupportHelper
    {
        public static MwiThemeInfo GetActualTheme(this IColorThemeSupport obj)
        {
            if (obj.Theme != null) return obj.Theme;
            var d = obj as DependencyObject;
            var a1 = d.GetVisualParents().OfType<IColorThemeSupport>().FirstOrDefault(a => !Equals(a, obj));
            return a1?.ActualTheme ?? MwiThemeInfo.DefaultTheme;
        }

        public static Color GetActualThemeColor(this IColorThemeSupport obj)
        {
            if (obj.ActualTheme.FixedColor != null) return obj.ActualTheme.FixedColor.Value;
            var d = obj as DependencyObject;
            if (obj.ThemeColor.HasValue) return obj.ThemeColor.Value;
            var a1 = d.GetVisualParents().OfType<IColorThemeSupport>().FirstOrDefault(a => !Equals(a, obj));
            return a1?.ActualThemeColor ?? MwiThemeInfo.DefaultThemeColor;
        }

        public static void UpdateColorTheme(this IColorThemeSupport obj, bool colorChanged, bool processChildren)
        {
            var fe = obj as FrameworkElement;
            if (fe.IsElementDisposing()) return;

            obj.UpdateColorTheme(colorChanged, processChildren);

            // obj.PropertyChanged.Invoke(fe, new PropertyChangedEventArgs("ActualTheme"));
            // obj.OnPropertiesChanged(nameof(ActualTheme), nameof(ActualThemeColor));

            if (processChildren)
                foreach (var element in fe.GetVisualChildren().OfType<IColorThemeSupport>())
                    element.UpdateColorTheme(colorChanged, false);
        }

        public static void SelectTheme(this IColorThemeSupport obj)
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

            var adorner = new DialogAdorner { CloseOnClickBackground = false };
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
                LimitPositionToPanelBounds = true,
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
