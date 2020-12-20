using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace WpfInvestigate.Effects
{
    public class FocusVisualEffect
    {
        #region ==============  Static Section  ==============
        static FocusVisualEffect()
        {
            EventManager.RegisterClassHandler(typeof(UIElement), Keyboard.PreviewGotKeyboardFocusEvent, (KeyboardFocusChangedEventHandler)OnPreviewGotKeyboardFocus);
            EventManager.RegisterClassHandler(typeof(UIElement), Keyboard.PreviewLostKeyboardFocusEvent, (KeyboardFocusChangedEventHandler)OnPreviewLostKeyboardFocus);
        }

        private static UIElement _lastFocusedElement;
        private static readonly PropertyInfo AlwaysShowFocusVisualPropertyInfo = typeof(KeyboardNavigation).GetProperty("AlwaysShowFocusVisual", BindingFlags.Static | BindingFlags.NonPublic);
        private static void OnPreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!Equals(e.NewFocus, _lastFocusedElement) && e.Source == e.NewFocus &&
                !IsKeyboardMostRecentInputDevice() && e.NewFocus is UIElement element && GetAlwaysShowFocus(element))
            {
                _lastFocusedElement = element;
                AlwaysShowFocusVisualPropertyInfo.SetValue(null, true);
            }
        }
        private static void OnPreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Equals(e.OldFocus, _lastFocusedElement) && e.Source == e.OldFocus)
            {
                _lastFocusedElement = null;
                AlwaysShowFocusVisualPropertyInfo.SetValue(null, SystemParameters.KeyboardCues);
            }
        }
        private static bool IsKeyboardMostRecentInputDevice() => InputManager.Current.MostRecentInputDevice is KeyboardDevice;
        #endregion

        #region ==============  Properties  ==============
        public static readonly DependencyProperty AlwaysShowFocusProperty = DependencyProperty.RegisterAttached(
            "AlwaysShowFocus", typeof(bool), typeof(FocusVisualEffect), new UIPropertyMetadata(false));
        public static bool GetAlwaysShowFocus(DependencyObject obj) => (bool)obj.GetValue(AlwaysShowFocusProperty);
        public static void SetAlwaysShowFocus(DependencyObject obj, bool value) => obj.SetValue(AlwaysShowFocusProperty, value);
        #endregion
    }
}
