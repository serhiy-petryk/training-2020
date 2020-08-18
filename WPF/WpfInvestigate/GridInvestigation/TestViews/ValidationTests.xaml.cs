using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GridInvestigation.TestViews
{
    /// <summary>
    /// Interaction logic for ValidationTests.xaml
    /// </summary>
    public partial class ValidationTests : Window
    {
        public ValidationTests()
        {
            InitializeComponent();
            dataGrid1.InitializingNewItem += (sender, e) =>
            {
                var newCourse = e.NewItem as Course;
                newCourse.StartDate = newCourse.EndDate = DateTime.Today;
            };
        }
    }

    public class Courses : ObservableCollection<Course>
    {
        public Courses()
        {
            Add(new Course
            {
                Name = "Learning WPF",
                Id = 1001,
                StartDate = new DateTime(2010, 1, 11),
                EndDate = new DateTime(2010, 1, 22)
            });
            Add(new Course
            {
                Name = "Learning Silverlight",
                Id = 1002,
                StartDate = new DateTime(2010, 1, 25),
                EndDate = new DateTime(2010, 2, 5)
            });
            Add(new Course
            {
                Name = "Learning Expression Blend",
                Id = 1003,
                StartDate = new DateTime(2010, 2, 8),
                EndDate = new DateTime(2010, 2, 19)
            });
            Add(new Course
            {
                Name = "Learning LINQ",
                Id = 1004,
                StartDate = new DateTime(2010, 2, 22),
                EndDate = new DateTime(2010, 3, 5)
            });
        }
    }

    public class Course : IEditableObject, INotifyPropertyChanged
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
            }
        }

        #region IEditableObject

        private Course backupCopy;
        private bool inEdit;

        public void BeginEdit()
        {
            if (inEdit) return;
            inEdit = true;
            backupCopy = MemberwiseClone() as Course;
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

    }

    public class CourseValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            var course = (value as BindingGroup).Items[0] as Course;
            if (course.StartDate > course.EndDate)
                return new ValidationResult(false, "Start Date must be earlier than End Date.");
            return ValidationResult.ValidResult;
        }
    }
}

