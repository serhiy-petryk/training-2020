using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using WpfInvestigate.Helpers;

namespace WpfInvestigate
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //Евент для оповещения всех окон приложения
        public static event EventHandler LanguageChanged;

        public static CultureInfo Language
        {
            get => Thread.CurrentThread.CurrentUICulture;
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (Equals(value, Thread.CurrentThread.CurrentUICulture)) return;

                //1. Меняем язык приложения:
                Thread.CurrentThread.CurrentUICulture = value;

                //2. Создаём ResourceDictionary для новой культуры
                var dict = new ResourceDictionary();
                switch (value.Name)
                {
                    case "ru-RU":
                    case "uk-UA":
                        // dict.Source = new Uri(String.Format("Resources/lang.{0}.xaml", value.Name), UriKind.Relative);
                        dict.Source = new Uri("Resources/lang.ru-RU.xaml", UriKind.Relative);
                        break;
                    default:
                        dict.Source = new Uri("Resources/lang.xaml", UriKind.Relative);
                        break;
                }

                // new ResourceDictionary { Source = new Uri(uri, UriKind.RelativeOrAbsolute) }
                //3. Находим старую ResourceDictionary и удаляем его и добавляем новую ResourceDictionary
                /* ResourceDictionary oldDict = (from d in Application.Current.Resources.MergedDictionaries
                                              where d.Source != null && d.Source.OriginalString.StartsWith("Resources/lang.")
                                              select d).First();
                if (oldDict != null)
                {
                    int ind = Application.Current.Resources.MergedDictionaries.IndexOf(oldDict);
                    Application.Current.Resources.MergedDictionaries.Remove(oldDict);
                    Application.Current.Resources.MergedDictionaries.Insert(ind, dict);
                }
                else
                {
                    Application.Current.Resources.MergedDictionaries.Add(dict);
                }*/

                FillResources(Current, dict);

                //4. Вызываем евент для оповещения всех окон.
                LanguageChanged?.Invoke(Application.Current, new EventArgs());
            }
        }

        private static void FillResources(Application app, ResourceDictionary resources)
        {
            foreach (var rd in resources.MergedDictionaries)
                FillResources(app, rd);
            foreach (var key in resources.Keys.OfType<string>())
                app.Resources[key] = resources[key];
        }
        //==================



        protected override void OnStartup(StartupEventArgs e)
        {
            var vCulture = new CultureInfo("uk");
            // var vCulture = Tips.InvariantCulture;

            Thread.CurrentThread.CurrentCulture = vCulture;
            Thread.CurrentThread.CurrentUICulture = vCulture;
            CultureInfo.DefaultThreadCurrentCulture = vCulture;
            CultureInfo.DefaultThreadCurrentUICulture = vCulture;

            var a1 = Thread.CurrentThread.CurrentCulture;
            var a2 = Thread.CurrentThread.CurrentUICulture;
            var a3 = CultureInfo.DefaultThreadCurrentCulture;
            var a4 = CultureInfo.DefaultThreadCurrentUICulture;

            // Apply culture to WPF controls (datepicker, ..)
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag)));

            // global event handlers 
            EventManager.RegisterClassHandler(typeof(ToolTip), ToolTip.OpenedEvent, new RoutedEventHandler(OnToolTipOpened));
            EventManager.RegisterClassHandler(typeof(ContextMenu), ContextMenu.OpenedEvent, new RoutedEventHandler(OnContextMenuOpened));
            EventManager.RegisterClassHandler(typeof(ToggleButton), ToggleButton.CheckedEvent, new RoutedEventHandler(OnToggleButtonChecked));

            SelectAllOnFocusForTextBox.ActivateGlobally();

            base.OnStartup(e);
        }

        private void OnContextMenuOpened(object sender, RoutedEventArgs e)
        {
            if (!(sender is ContextMenu contextMenu)) return;
            if (!(contextMenu.PlacementTarget is FrameworkElement owner)) return;
            contextMenu.ApplyTransform(owner);
        }
        private void OnToolTipOpened(object sender, RoutedEventArgs e)
        {
            if (!(sender is ToolTip toolTip)) return;
            if (!(toolTip.PlacementTarget is FrameworkElement owner)) return;
            toolTip.ApplyTransform(owner);
        }

        private void OnToggleButtonChecked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton button && Equals(button.IsChecked, true))
            {
                var cm = button.Tag as ContextMenu ?? button.Resources.Values.OfType<ContextMenu>().FirstOrDefault();
                if (cm != null && !cm.IsOpen) // ContextMenu may be already opened (?bug (binding mode=TwoWay=>twice event call when value changed), see SplitButtonStyle)
                {
                    if (cm.PlacementTarget == null)
                    {
                        cm.PlacementTarget = button;
                        cm.Placement = PlacementMode.Bottom;
                        cm.Closed += (senderClosed, eClosed) => ((ToggleButton)sender).IsChecked = false;
                    }
                    cm.IsOpen = true;
                }
            }
        }
    }
}
