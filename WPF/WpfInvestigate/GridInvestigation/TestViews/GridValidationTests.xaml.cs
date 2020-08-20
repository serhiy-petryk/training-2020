using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace GridInvestigation.TestViews
{
    /// <summary>
    /// Interaction logic for GridValidationTests.xaml
    /// </summary>
    public partial class GridValidationTests : Window
    {
        public GridValidationTests()
        {
            InitializeComponent();
            dataGrid1.InitializingNewItem += (sender, e) =>
            {
                var newCourse = e.NewItem as Course5;
                newCourse.StartDate = newCourse.EndDate = DateTime.Today;
            };
        }
    }

    public class Courses5 : ObservableCollection<Course5>
    {
        public Courses5()
        {
            Add(new Course5
            {
                Name = "Learning WPF",
                Id = 1001,
                StartDate = new DateTime(2010, 1, 11),
                EndDate = new DateTime(2010, 1, 22)
            });
            Add(new Course5
            {
                Name = "Learning Silverlight",
                Id = 1002,
                StartDate = new DateTime(2010, 1, 25),
                EndDate = new DateTime(2010, 2, 5)
            });
            Add(new Course5
            {
                Name = "Learning Expression Blend",
                Id = 1003,
                StartDate = new DateTime(2010, 2, 8),
                EndDate = new DateTime(2010, 2, 19)
            });
            Add(new Course5
            {
                Name = "Learning LINQ",
                Id = 1004,
                StartDate = new DateTime(2010, 2, 22),
                EndDate = new DateTime(2010, 3, 5)
            });
        }
    }

    public class Course5 : INotifyPropertyChanged, IDataErrorInfo, IEditableObject
    {
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private int _number;
        public int Id
        {
            get => _number;
            set
            {
                if (_number == value) return;
                _number = value;
                OnPropertyChanged("Id");
            }
        }

        private DateTime _startDate;
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate == value) return;
                _startDate = value;
                OnPropertyChanged("StartDate");
                OnPropertyChanged("EndDate"); // for cross-column validation
            }
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate == value) return;
                _endDate = value;
                OnPropertyChanged("EndDate");
                OnPropertyChanged("StartDate"); // for cross-column validation
            }
        }

        #region IEditableObject

        private Course5 backupCopy;
        private bool inEdit;

        public void BeginEdit()
        {
            if (inEdit) return;
            inEdit = true;
            backupCopy = MemberwiseClone() as Course5;
        }

        public void CancelEdit()
        {
            if (!inEdit) return;
            inEdit = false;
            Name = backupCopy.Name;
            Id = backupCopy.Id;
            StartDate = backupCopy.StartDate;
            EndDate = backupCopy.EndDate;
        }

        public void EndEdit()
        {
            if (!inEdit) return;
            inEdit = false;
            backupCopy = null;
        }

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        #region =====  IDataErrorInfo  =======
        public string Error => StartDate > EndDate ? "Start Date must be earlier than End Date" + Environment.NewLine + "Item error" : null;
        public string this[string propertyName] =>
            ((propertyName == nameof(StartDate) || propertyName == nameof(EndDate)) && StartDate > EndDate)
                ? "Start Date must be earlier than End Date" + Environment.NewLine + "Property row"
                : null;
        #endregion
    }
}

