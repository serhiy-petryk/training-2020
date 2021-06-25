using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WpfSpLib.Common;
using WpfSpLib.Controls;

namespace WpfSpLibDemo.TestViews
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

            cbDataType2.ItemsSource = cbDataType.ItemsSource;
            cbDataType2.SelectedValue = DataTypeMetadata.DataType.String;
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
            ChangeValue(Editor);
        }

        private void ChangeValue2_OnClick(object sender, RoutedEventArgs e)
        {
            ChangeValue(Editor2);
            ChangeValue(Editor3);
        }

        private void ChangeValue(ObjectEditor editor)
        {
            if (editor.ValueDataType == DataTypeMetadata.DataType.Bool)
                editor.Value = !(bool)editor.Value;
            if (editor.ValueDataType == DataTypeMetadata.DataType.Byte)
                editor.Value = (byte)editor.Value + 1;
            if (editor.ValueDataType == DataTypeMetadata.DataType.String)
                editor.Value = (string)editor.Value + "a";
            if (editor.ValueDataType == DataTypeMetadata.DataType.DateTime)
                editor.Value = ((DateTime)editor.Value).AddHours(1);
        }
    }
}
