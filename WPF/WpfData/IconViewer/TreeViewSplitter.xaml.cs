using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace IconViewer
{
    /// <summary>
    /// Interaction logic for TreeViewSplitter.xaml
    /// </summary>
    public partial class TreeViewSplitter : UserControl
    {
        public static List<Department> deptList = new List<Department>();

        public List<Student> studentList = new List<Student>();

        public TreeViewSplitter()
        {
            InitializeComponent();

            List<Subject> sublist1 = new List<Subject>();
            sublist1.Add(new Subject(100101, "Pre Calculus"));
            sublist1.Add(new Subject(100210, "Calculus 1"));
            sublist1.Add(new Subject(100211, "Calculus 2"));
            sublist1.Add(new Subject(100212, "Calculus 3"));
            sublist1.Add(new Subject(100218, "Linear Algebra"));
            sublist1.Add(new Subject(100213, "Differential Equation"));
            Department dept1 = new Department();
            dept1.Name = "Math";
            dept1.Subjects = sublist1;

            deptList.Add(dept1);

            List<Subject> sublist2 = new List<Subject>();
            sublist2.Add(new Subject(200100, "Business Accounting"));
            sublist2.Add(new Subject(200101, "Accounting 1"));
            sublist2.Add(new Subject(200102, "Accounting 2"));
            sublist2.Add(new Subject(200201, "Accounting 3"));
            sublist2.Add(new Subject(200201, "Accounting 4"));
            sublist2.Add(new Subject(200203, "Cost Accounting"));
            sublist2.Add(new Subject(200205, "Tax Accounting"));
            sublist2.Add(new Subject(200214, "Auditing"));
            Department dept2 = new Department();
            dept2.Name = "Accounting";
            dept2.Subjects = sublist2;

            deptList.Add(dept2);

            List<Subject> sublist3 = new List<Subject>();
            sublist3.Add(new Subject(300101, "Introduction to CS"));
            sublist3.Add(new Subject(300106, "OOP"));
            sublist3.Add(new Subject(300121, "Visual Basic"));
            sublist3.Add(new Subject(300170, "Security 1"));
            sublist3.Add(new Subject(300201, "Computer Science"));
            sublist3.Add(new Subject(300208, "C++ Programming"));
            sublist3.Add(new Subject(300232, "DB Administrator"));
            sublist3.Add(new Subject(300241, "Networking"));
            Department dept3 = new Department();
            dept3.Name = "Computer Science";
            dept3.Subjects = sublist3;

            deptList.Add(dept3);

            studentList.Add(new Student(10, "Alex", "Martin", 100210, "A"));
            studentList.Add(new Student(10, "Alex", "Martin", 300101, "A"));
            studentList.Add(new Student(10, "Alex", "Martin", 200101, "B"));
            studentList.Add(new Student(12, "Joe", "Brown", 100101, "A"));
            studentList.Add(new Student(12, "Joe", "Brown", 300170, "B"));
            studentList.Add(new Student(12, "Joe", "Brown", 200203, "A"));
            studentList.Add(new Student(16, "David", "Frank", 100212, "A"));
            studentList.Add(new Student(16, "David", "Frank", 100218, "A"));
            studentList.Add(new Student(16, "David", "Frank", 100213, "A"));
            studentList.Add(new Student(16, "David", "Frank", 300101, "A"));
            studentList.Add(new Student(16, "David", "Frank", 200100, "B"));
            studentList.Add(new Student(18, "Patrick", "Justin", 200100, "A"));
            studentList.Add(new Student(18, "Patrick", "Justin", 300101, "A"));
            studentList.Add(new Student(18, "Patrick", "Justin", 100210, "A"));
            studentList.Add(new Student(18, "Patrick", "Justin", 100211, "A"));
            studentList.Add(new Student(25, "Ken", "White", 100210, "B"));
            studentList.Add(new Student(25, "Ken", "White", 300101, "B"));
            studentList.Add(new Student(25, "Ken", "White", 200101, "A"));

        }

        private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            list.Items.Clear();

            Subject selectedSubject = tree.SelectedItem as Subject;

            if (selectedSubject != null)
            {
                int subjectID = selectedSubject.ID;

                IEnumerable<Student> selectedStudents = from fn in studentList
                                                        where fn.SubjectID == subjectID
                                                        select fn;

                foreach (Student student in selectedStudents)
                {
                    list.Items.Add(student);
                }
            }
        }
    }

    public class Subject
    {
        public Subject(int id, String name)
        {
            ID = id;
            Name = name;
        }

        public int ID
        { get; set; }

        public String Name
        { get; set; }
    }

    public class Department
    {
        public Department()
        {
            this.Subjects = new List<Subject>();
        }

        public String Name
        { get; set; }

        public List<Subject> Subjects
        { get; set; }
    }

    public class Student
    {
        public Student()
        {
        }

        public Student(int id, String firstName, String lastName, int subjectID, String grade)
        {
            ID = id;
            FirstName = firstName;
            LastName = lastName;
            SubjectID = subjectID;
            Grade = grade;
        }

        public int ID
        { get; set; }

        public String FirstName
        { get; set; }

        public String LastName
        { get; set; }

        public int SubjectID
        { get; set; }

        public String Grade
        { get; set; }
    }
}
