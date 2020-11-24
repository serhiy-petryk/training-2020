using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WpfInvestigate.Common;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for ObjectEditorTest.xaml
    /// </summary>
    public partial class ObjectEditorTests
    {
        public ObjectEditorTests()
        {
            InitializeComponent();
            DataContext = this;
            cbDataType.ItemsSource = Enum.GetValues(typeof(DataTypeMetadata.DataType)).Cast<DataTypeMetadata.DataType>();
            cbDataType.SelectedValue = DataTypeMetadata.DataType.Date;
        }

        public IEnumerable<string> DataTypes => Enum.GetNames(typeof(DataTypeMetadata.DataType)).Select(a=> a.ToString());

        private void Test_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = Editor.Value == null ? "null" : Editor.Value.GetType().Name;
            MessageBox.Show(a1);
            // Editor.Value = "123";
        }

        private void ChangeValue_OnClick(object sender, RoutedEventArgs e)
        {
            if (Editor.ValueDataType == DataTypeMetadata.DataType.Bool)
                Editor.Value = !(bool)Editor.Value;
            if (Editor.ValueDataType == DataTypeMetadata.DataType.Byte)
                Editor.Value = (byte)Editor.Value + 1;
            if (Editor.ValueDataType == DataTypeMetadata.DataType.String)
                Editor.Value = (string)Editor.Value + "a";
            if (Editor.ValueDataType == DataTypeMetadata.DataType.DateTime)
                Editor.Value = ((DateTime)Editor.Value).AddHours(1);
        }
    }
}
