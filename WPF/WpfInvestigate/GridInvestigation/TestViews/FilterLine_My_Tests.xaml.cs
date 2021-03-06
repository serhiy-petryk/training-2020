﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace GridInvestigation.TestViews
{
    /// <summary>
    /// Interaction logic for FilterLine_My_Tests.xaml
    /// </summary>
    public partial class FilterLine_My_Tests : Window
    {
        public FilterLine_My_Tests()
        {
            InitializeComponent();
        }
    }

    public class Lines3 : ObservableCollection<FilterLineSubitem3>
    {
        public Lines3()
        {
        }
    }

    public class FilterLineSubitem3 : INotifyPropertyChanged, IDataErrorInfo, IEditableObject
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
                    RefreshUI();
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
                    RefreshUI();
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
                    RefreshUI();
                }
            }
        }

        private void RefreshUI()
        {
            OnPropertiesChanged(nameof(Operand), nameof(Value1), nameof(Value2), nameof(Error));
        }

        #region ===========  IDataErrorInfo  ===========
        public string this[string propertyName]
        {
            get
            {
                if (!string.IsNullOrEmpty(_operand) && _value1 == null && propertyName == "Value1") return "Вкажіть вираз №1";
                if (string.IsNullOrEmpty(_operand) && _value1 != null && propertyName == "Value1") return "Зітріть вираз №1";
                if (!string.IsNullOrEmpty(_operand) && _value2 == null && propertyName == "Value2") return "Вкажіть вираз №2";
                if (string.IsNullOrEmpty(_operand) && _value2 != null && propertyName == "Value2") return "Зітріть вираз №2";
                return null;
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
                return string.IsNullOrEmpty(a1) ? null : a1;
            }
        }

        #endregion

        #region ===========  INotifyPropertyChanged  ===========
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region ===========  IEditableObject  ===========
        private FilterLineSubitem3 backupCopy;
        private bool inEdit;
        public void BeginEdit()
        {
            if (inEdit) return;
            inEdit = true;
            backupCopy = MemberwiseClone() as FilterLineSubitem3;
        }

        public void EndEdit()
        {
            if (!inEdit) return;
            inEdit = false;
            backupCopy = null;
        }

        public void CancelEdit()
        {
            if (!inEdit) return;
            inEdit = false;
            Operand = backupCopy.Operand;
            Value1 = backupCopy.Value1;
            Value2 = backupCopy.Value2;
        }
        #endregion

    }
}

