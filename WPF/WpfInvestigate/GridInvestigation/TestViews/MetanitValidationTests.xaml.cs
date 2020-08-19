//========= https://metanit.com/sharp/wpf/14.php =========

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace GridInvestigation.TestViews
{
    /// <summary>
    /// Interaction logic for MetanitValidationTests.xaml
    /// </summary>
    public partial class MetanitValidationTests : Window
    {
        PersonModel Tom;
        public MetanitValidationTests()
        {
            InitializeComponent();
            Tom = new PersonModel();
            DataContext = Tom;
        }

        private void TextBox_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                MessageBox.Show(e.Error.ErrorContent.ToString());
        }
    }

    public class PersonModel: IDataErrorInfo
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Position { get; set; }

        public string this[string columnName]
        {
            get
            {
                var error = string.Empty;
                switch (columnName)
                {
                    case "Age":
                        if ((Age < 0) || (Age > 100)) error = "Возраст должен быть больше 0 и меньше 100";
                        break;
                    case "Name":
                        //Обработка ошибок для свойства Name
                        break;
                    case "Position":
                        //Обработка ошибок для свойства Position
                        break;
                }
                return error;
            }
        }
        public string Error => throw new NotImplementedException();

        public override string ToString() => $"Name: {Name}, Age: {Age}, Position: {Position}";
    }
}
