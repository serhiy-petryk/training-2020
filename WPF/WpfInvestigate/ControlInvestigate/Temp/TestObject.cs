using System.Windows;
using System.Windows.Controls;

namespace ControlInvestigate.Temp
{
    public class TestObject: Control
    {
        static TestObject()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TestObject), new FrameworkPropertyMetadata(typeof(TestObject)));
        }

        public string TestProperty => "Test";

        public TestObject()
        {
            DataContext = this;
        }

    }
}
