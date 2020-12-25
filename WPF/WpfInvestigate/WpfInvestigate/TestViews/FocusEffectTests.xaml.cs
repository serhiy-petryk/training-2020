using System.Windows;
using WpfInvestigate.Helpers;

namespace WpfInvestigate.TestViews
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
