using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace GridInvestigation.TestViews
{
    /// <summary>
    /// Interaction logic for NestedPropertiesTest.xaml
    /// </summary>
    public partial class NestedPropertiesTest : Window
    {
        public NestedPropertiesTest()
        {
            InitializeComponent();
            var level6 = new Models.Level6( );
            var level5 = new Models.Level5 { Level6 = level6 };
            var level4 = new Models.Level4 { Id = "Level4", Level5 = level5 };
            var level3 = new Models.Level3 { Id = "Level3", Level4 = level4 };
            var level2 = new Models.Level2 { Id = "Level2", Level3 = level3};
            var level11 = new Models.Level1 { Id = "Level1", Level2 = level2, Bool = true };
            var level12 = new Models.Level1 { Id = "Level1", Level2 = level2, Bool = null };
            var items = new List<object>();
            items.Add(level11);
            items.Add(level12);
            items.Add(new object());
            DG.ItemsSource = items;
        }
    }
}
