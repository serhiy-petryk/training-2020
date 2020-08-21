// this => INotifyPropertyChanged+INotifyDataErrorInfo: https://stackoverflow.com/questions/34665650/force-inotifydataerrorinfo-validation =========

// Exception in setter: https://www.nbdtech.com/Blog/archive/2010/07/05/wpf-adorners-part-3-ndash-adorners-and-validation.aspx
// -- Not working!!! Attributes and INotifyPropertyChanged+INotifyDataErrorInfo: https://www.c-sharpcorner.com/UploadFile/tirthacs/inotifydataerrorinfo-in-wpf/

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace GridInvestigation.TestViews
{
    /// <summary>
    /// Interaction logic for INotifyDataErrorInfoValidationTests.xaml
    /// </summary>
    public partial class INotifyDataErrorInfoValidationTests : Window
    {
        PersonModel1 Tom;
        public INotifyDataErrorInfoValidationTests()
        {
            InitializeComponent();
            Tom = new PersonModel1();
            DataContext = Tom;
        }

        private void TextBox_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                MessageBox.Show(e.Error.ErrorContent.ToString());
        }
    }

    public class PersonModel1: ModelBase1
    {
        private string _name;
        [System.ComponentModel.DataAnnotations.StringLength(4)]
        public string Name { get=>_name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _age;
        public int Age { 
            get => _age;
            set
            {
                if (_age != value)
                {
                    _age = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Position { get; set; }

        public override IEnumerable GetErrors([CallerMemberName] string propertyName = null)
        {
            Debug.Print($"GetErrors: {propertyName}");
            if (string.IsNullOrEmpty(propertyName) || propertyName == nameof(Age))
            {
                if (Age < 0 || Age > 100)
                    yield return "Возраст должен быть больше 0 и меньше 100";
            }
        }

        public override string ToString() => $"Name: {Name}, Age: {Age}, Position: {Position}";
    }

    public class ModelBase1 : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region =====  INotifyPropertyChanged  =======
        public event PropertyChangedEventHandler PropertyChanged; // INotifyPropertyChanged
        #endregion

        #region =====  INotifyDataErrorInfo  =======
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public bool HasErrors => GetErrors(null).OfType<object>().Any();
        public virtual IEnumerable GetErrors([CallerMemberName] string propertyName = null) => Enumerable.Empty<object>();
        #endregion

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
