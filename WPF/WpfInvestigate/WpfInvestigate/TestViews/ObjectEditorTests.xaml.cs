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
    }
}
