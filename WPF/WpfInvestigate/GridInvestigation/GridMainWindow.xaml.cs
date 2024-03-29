﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using GridInvestigation.TestViews;

namespace GridInvestigation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class GridMainWindow : Window
    {
        public GridMainWindow()
        {
            InitializeComponent();
            DataContext = this;
            cbCulture.ItemsSource = CultureInfos;
            cbCulture.SelectedValue = Thread.CurrentThread.CurrentUICulture;
        }

        private static string[] _cultures = { "", "sq-AL", "uk-UA", "en-US", "km-KH", "yo-NG" };

        public List<CultureInfo> CultureAllInfos { get; set; } = CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures).OrderBy(c => c.DisplayName).ToList();
        public List<CultureInfo> CultureInfos { get; set; } = CultureInfo
            .GetCultures(CultureTypes.InstalledWin32Cultures).Where(c => Array.IndexOf(_cultures, c.Name) != -1)
            .OrderBy(c => c.DisplayName).ToList();

        private void CbCulture_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                var newCulture = e.AddedItems[0] as CultureInfo;
                Thread.CurrentThread.CurrentCulture = newCulture; // MainCulture for DatePicker
                Thread.CurrentThread.CurrentUICulture = newCulture;
                CultureInfo.DefaultThreadCurrentCulture = newCulture;
                CultureInfo.DefaultThreadCurrentUICulture = newCulture;
            }
        }

        private void OnTestButtonClick(object sender, RoutedEventArgs e)
        {
        }

        private void ValidationTest_OnClick(object sender, RoutedEventArgs e) => new ValidationTests().Show();
        private void MetanitValidationTest_OnClick(object sender, RoutedEventArgs e) => new MetanitValidationTests().Show();
        private void MyValidationTest_OnClick(object sender, RoutedEventArgs e) => new MyValidationTests().Show();
        private void AttributeValidationTest_OnClick(object sender, RoutedEventArgs e) => new AttributeValidationTests().Show();
        private void IDataErrorInfoValidationTests_OnClick(object sender, RoutedEventArgs e) => new IDataErrorInfoValidationTests().Show();
        private void INotifyDataErrorInfoValidationTests_OnClick(object sender, RoutedEventArgs e) => new INotifyDataErrorInfoValidationTests().Show();
        private void GridValidationTests_OnClick(object sender, RoutedEventArgs e) => new GridValidationTests().Show();
        private void RedefineInternalValidationTests_OnClick(object sender, RoutedEventArgs e) => new RedefineInternalValidationTests().Show();
        private void FilterLine_IDataErrorInfo_Tests_OnClick(object sender, RoutedEventArgs e) => new FilterLine_IDataErrorInfo_Tests().Show();
        private void FilterLine_INotifyDataErrorInfo_Tests_OnClick(object sender, RoutedEventArgs e) => new FilterLine_INotifyDataErrorInfo_Tests().Show();
        private void FilterLine_My_Tests_OnClick(object sender, RoutedEventArgs e) => new FilterLine_My_Tests().Show();

        private void NestedProperties_OnClick(object sender, RoutedEventArgs e) => new NestedPropertiesTest().Show();

        private void ImageColumnTest_OnClick(object sender, RoutedEventArgs e) => new ImageColumnTest().Show();
    }
}
