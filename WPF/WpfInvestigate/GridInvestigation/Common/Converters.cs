﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GridInvestigation.Common
{
    public class DummyConverter : DependencyObject, IValueConverter
    {
        public static DummyConverter Instance = new DummyConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class OpacityForDataGridRowHeader : DependencyObject, IValueConverter
    {
        public static OpacityForDataGridRowHeader Instance = new OpacityForDataGridRowHeader();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value.GetType().Name == "NamedObject" => new row ({{NewItemPlaceholder}}) in DataGrid
            return value == null || Equals(value.GetType().Name, "NamedObject") ? 0.0 : 1.0;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class VisibilityConverter : IValueConverter
    {
        public static VisibilityConverter Instance = new VisibilityConverter();
        public static VisibilityConverter InverseInstance = new VisibilityConverter { _inverse = true };
        private bool _inverse;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _inverse ^ (value == null || Equals(value, false) || Equals(value, 0) || Equals(value, ""))
                ? (Equals(parameter, "Hide") ? Visibility.Hidden : Visibility.Collapsed)
                : Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }


}
