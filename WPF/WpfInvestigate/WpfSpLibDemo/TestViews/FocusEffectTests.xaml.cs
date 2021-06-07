using System.Windows;
using WpfSpLib.Helpers;

namespace WpfSpLibDemo.TestViews
{
    /// <summary>
    /// Interaction logic for FocusEffectTests.xaml
    /// </summary>
    public partial class FocusEffectTests : Window
    {
        public FocusEffectTests()
        {
            InitializeComponent();
            ControlHelper.HideInnerBorderOfDatePickerTextBox(this, true);
        }
    }
}
