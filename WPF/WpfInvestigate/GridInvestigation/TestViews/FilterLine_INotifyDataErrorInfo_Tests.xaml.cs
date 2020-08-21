using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace GridInvestigation.TestViews
{
    /// <summary>
    /// Interaction logic for FilterLine_INotifyDataErrorInfo_Tests.xaml
    /// </summary>
    public partial class FilterLine_INotifyDataErrorInfo_Tests : Window
    {
        public FilterLine_INotifyDataErrorInfo_Tests()
        {
            InitializeComponent();
        }
    }

    public class Lines2 : ObservableCollection<FilterLineSubitem2>
    {
        public Lines2()
        {
        }
    }

    public class FilterLineSubitem2 : INotifyPropertyChanged, INotifyDataErrorInfo, IEditableObject
    {
        //private Common.Enums.FilterOperand _operand;
        private string _operand;
        private object _value1;
        private object _value2;

        public string Operand
        {
            get => _operand;
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = null;

                if (_operand != value)
                {
                    _operand = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Value1));
                    OnPropertyChanged(nameof(Value2));
                    OnPropertyChanged(nameof(Error));
                    OnPropertyChanged(nameof(HasErrors));
                }
            }
        }

        public object Value1
        {
            get => _value1;
            set
            {
                if (value is string && string.IsNullOrEmpty((string)value))
                    value = null;
                if (_value1 != value)
                {
                    _value1 = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Operand));
                    OnPropertyChanged(nameof(Error));
                    OnPropertyChanged(nameof(HasErrors));
                }
            }
        }

        public object Value2
        {
            get => _value2;
            set
            {
                if (value is string && string.IsNullOrEmpty((string)value))
                    value = null;
                if (_value2 != value)
                {
                    _value2 = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Operand));
                    OnPropertyChanged(nameof(Error));
                    OnPropertyChanged(nameof(HasErrors));
                }
            }
        }

        public string Error
        {
            get
            {
                var sb = new StringBuilder();
                if (!string.IsNullOrEmpty(_operand) && _value1 == null) sb.AppendLine("Вкажіть вираз №1");
                if (string.IsNullOrEmpty(_operand) && _value1 != null) sb.AppendLine("Зітріть вираз №1");
                if (!string.IsNullOrEmpty(_operand) && _value2 == null) sb.AppendLine("Вкажіть вираз №2");
                if (string.IsNullOrEmpty(_operand) && _value2 != null) sb.AppendLine("Зітріть вираз №2");
                var a1 = sb.ToString().Trim();
                Debug.Print($"Error: {a1}");
                return string.IsNullOrEmpty(a1) ? null : a1;
            }
        }

        #region ===========  INotifyPropertyChanged  ===========
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Debug.Print($"OnPropertyChanged: {propertyName}");
            if (propertyName == "Error")
            {
                Debug.Print($"OnErrorPropertyChanged: {Error}");
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ===========  INotifyDataErrorInfo  ===========
        public IEnumerable GetErrors([CallerMemberName] string propertyName = null)
        {
            Debug.Print($"GetErrors: {propertyName}");
            if (!string.IsNullOrEmpty(_operand) && _value1 == null && propertyName == "Value1") yield return "Вкажіть вираз №1";
            if (string.IsNullOrEmpty(_operand) && _value1 != null && propertyName == "Value1") yield return "Зітріть вираз №1";
            if (!string.IsNullOrEmpty(_operand) && _value2 == null && propertyName == "Value2") yield return "Вкажіть вираз №2";
            if (string.IsNullOrEmpty(_operand) && _value2 != null && propertyName == "Value2") yield return "Зітріть вираз №2";
        }

        public bool HasErrors => GetErrors($"Operand").OfType<object>().Any() ||
                                 GetErrors($"Value1").OfType<object>().Any() ||
                                 GetErrors($"Value2").OfType<object>().Any();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        #endregion

        #region ===========  IEditableObject  ===========
        private FilterLineSubitem2 backupCopy;
        private bool inEdit;
        public void BeginEdit()
        {
            if (inEdit) return;
            inEdit = true;
            backupCopy = MemberwiseClone() as FilterLineSubitem2;
            Debug.Print($"BeginEdit");
        }

        public void EndEdit()
        {
            if (!inEdit) return;
            inEdit = false;
            backupCopy = null;
            OnPropertyChanged(nameof(Value1));
            OnPropertyChanged(nameof(Value2));
            OnPropertyChanged(nameof(Operand));
            OnPropertyChanged(nameof(Error));
            OnPropertyChanged(nameof(HasErrors));
            Debug.Print($"EndEdit");
        }

        public void CancelEdit()
        {
            if (!inEdit) return;
            inEdit = false;
            Operand = backupCopy.Operand;
            Value1 = backupCopy.Value1;
            Value2 = backupCopy.Value2;
            OnPropertyChanged(nameof(Value1));
            OnPropertyChanged(nameof(Value2));
            OnPropertyChanged(nameof(Operand));
            OnPropertyChanged(nameof(Error));
            OnPropertyChanged(nameof(HasErrors));
            Debug.Print($"CancelEdit");
        }
        #endregion
    }
}

