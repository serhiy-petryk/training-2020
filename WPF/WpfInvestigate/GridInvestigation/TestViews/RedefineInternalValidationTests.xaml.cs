// From https://joshsmithonwpf.wordpress.com/2008/11/14/using-a-viewmodel-to-provide-meaningful-validation-error-messages/
// How Redefine Internal Validation Tests: use proxy strings for numbers/dates, ...
// They are editing and convert to real numbers/dates from string proxies.

using System.ComponentModel;
using System.Windows;

namespace GridInvestigation.TestViews
{
    /// <summary>
    /// Interaction logic for RedefineInternalValidationTests.xaml
    /// </summary>
    public partial class RedefineInternalValidationTests : Window
    {
        public RedefineInternalValidationTests()
        {
            InitializeComponent();
            Person fordPrefect = new Person("Ford Prefect", 42);
            DataContext = new PersonViewModel(fordPrefect);
        }
    }

    //===========================
    public class Person : IDataErrorInfo
    {
        public Person(string name, int age)
        {
            Name = name;
            Age = age;
        }

        public string Name { get; private set; }
        public int Age { get; set; }

        #region IDataErrorInfo Members

        public string Error => null;

        public string this[string propertyName]
        {
            get
            {
                if (propertyName == "Age")
                {
                    if (Age < 0)
                        return "Age cannot be less than 0.";

                    if (120 < Age)
                        return "Age cannot be greater than 120.";
                }

                return null;
            }
        }

        #endregion // IDataErrorInfo Members
    }

    public class PersonViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        readonly Person _person;

        public PersonViewModel(Person person)
        {
            _person = person;
            _ageText = person.Age.ToString();
        }

        public string Name => _person.Name;

        string _ageText;
        public string Age
        {
            get => _ageText;
            set
            {
                if (value == _ageText)
                    return;
                _ageText = value;
                OnPropertyChanged("Age");
            }
        }

        #region IDataErrorInfo Members
        public string Error => _person.Error;
        public string this[string propertyName]
        {
            get
            {
                if (propertyName == "Age")
                {
                    int age;
                    var msg = ValidateAge(out age);
                    if (!string.IsNullOrEmpty(msg))
                        return msg;

                    // Apply the age value now so that the 
                    // Person object can also validate it.
                    _person.Age = age;
                }

                return _person[propertyName];
            }
        }
        private string ValidateAge(out int age)
        {
            age = -1;
            string msg = null;

            if (string.IsNullOrEmpty(_ageText))
                msg = "Age is missing.";

            if (!int.TryParse(_ageText, out age))
                msg = "Age is not a whole number.";

            return msg;
        }
        #endregion // IDataErrorInfo Members

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion // INotifyPropertyChanged Members
    }
}
