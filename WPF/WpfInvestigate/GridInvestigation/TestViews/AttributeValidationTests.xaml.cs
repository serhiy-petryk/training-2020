// this => INotifyPropertyChanged+INotifyDataErrorInfo: https://stackoverflow.com/questions/34665650/force-inotifydataerrorinfo-validation =========

// Exception in setter: https://www.nbdtech.com/Blog/archive/2010/07/05/wpf-adorners-part-3-ndash-adorners-and-validation.aspx
// ++ Attributes and INotifyPropertyChanged+INotifyDataErrorInfo: https://www.c-sharpcorner.com/UploadFile/tirthacs/inotifydataerrorinfo-in-wpf/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace GridInvestigation.TestViews
{
    /// <summary>
    /// Interaction logic for AttributeValidationTests.xaml
    /// </summary>
    public partial class AttributeValidationTests : Window
    {
        public Customer Customer { get; }

        public string BadProperty
        {
            get => "Bad";
            set
            {
                if (value != "Bad")
                    throw new Exception("Value must be \"Bad\"");
            }
        }

        public AttributeValidationTests()
        {
            InitializeComponent();
            DataContext = this;
            Customer = new Customer();
        }

        private void TextBox_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                MessageBox.Show(e.Error.ErrorContent.ToString());
        }
    }

    //=======================
    public class Customer : ModelBase2
    {
        private string _firstName;
        [Display(Name = "First Name")]
        [Required]
        [StringLength(20)]
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                _firstName = value;
                ValidateProperty(value);
                base.NotifyPropertyChanged("FirstName");
            }
        }
    }

    public class ModelBase2 : ValidationBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class ValidationBase : INotifyDataErrorInfo
    {
        private Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        private object _lock = new object();
        public bool HasErrors { get { return _errors.Any(propErrors => propErrors.Value != null && propErrors.Value.Count > 0); } }
        public bool IsValid { get { return this.HasErrors; } }

        public IEnumerable GetErrors(string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                if (_errors.ContainsKey(propertyName) && (_errors[propertyName] != null) && _errors[propertyName].Count > 0)
                    return _errors[propertyName].ToList();
                else
                    return null;
            }
            else
                return _errors.SelectMany(err => err.Value.ToList());
        }

        public void OnErrorsChanged(string propertyName)
        {
            if (ErrorsChanged != null)
                ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void ValidateProperty(object value, [CallerMemberName] string propertyName = null)
        {
            lock (_lock)
            {
                var validationContext = new ValidationContext(this, null, null);
                validationContext.MemberName = propertyName;
                var validationResults = new List<ValidationResult>();
                Validator.TryValidateProperty(value, validationContext, validationResults);

                //clear previous _errors from tested property  
                if (_errors.ContainsKey(propertyName))
                    _errors.Remove(propertyName);
                OnErrorsChanged(propertyName);
                HandleValidationResults(validationResults);
            }
        }

        public void Validate()
        {
            lock (_lock)
            {
                var validationContext = new ValidationContext(this, null, null);
                var validationResults = new List<ValidationResult>();
                Validator.TryValidateObject(this, validationContext, validationResults, true);

                //clear all previous _errors  
                var propNames = _errors.Keys.ToList();
                _errors.Clear();
                propNames.ForEach(pn => OnErrorsChanged(pn));
                HandleValidationResults(validationResults);
            }
        }

        private void HandleValidationResults(List<ValidationResult> validationResults)
        {
            //Group validation results by property names  
            var resultsByPropNames = from res in validationResults
                                     from mname in res.MemberNames
                                     group res by mname into g
                                     select g;
            //add _errors to dictionary and inform binding engine about _errors  
            foreach (var prop in resultsByPropNames)
            {
                var messages = prop.Select(r => r.ErrorMessage).ToList();
                _errors.Add(prop.Key, messages);
                OnErrorsChanged(prop.Key);
            }
        }
    }

}
