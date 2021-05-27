using System.Windows;
using WpfLib.Helpers;

namespace WpfLib.TestViews
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
