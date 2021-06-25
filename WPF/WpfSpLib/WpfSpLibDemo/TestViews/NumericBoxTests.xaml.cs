using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace WpfSpLibDemo.TestViews
{
    /// <summary>
    /// Interaction logic for NumericBoxTests.xaml
    /// </summary>
    public partial class NumericBoxTests
    {
        private static string[] _cultures = { "", "sq-AL", "uk-UA", "en-US" };

        public List<CultureInfo> CultureAllInfos { get; set; } = CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures).OrderBy(c => c.DisplayName).ToList();

        public List<CultureInfo> CultureInfos { get; set; } = CultureInfo
            .GetCultures(CultureTypes.InstalledWin32Cultures).Where(c => Array.IndexOf(_cultures, c.Name) != -1)
            .OrderBy(c => c.DisplayName).ToList();

        public NumericBoxTests()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void XButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            AA.Value = (AA.Value ?? 100M) + 10;
            AA.ButtonsWidth += 2;
        }
    }
}
