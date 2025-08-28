using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AppCore5.Helpers;
using AppCore5.Models;

namespace AppCore5
{
    /// <summary>
    /// Interaction logic for DataGridTest.xaml
    /// </summary>
    public partial class DataGridTest : Window
    {
        public DataGridTest()
        {
            InitializeComponent();

            var users = new List<User>();
            users.Add(new User() { Id = 1, Name = "John Doe", Birthday = new DateTime(1971, 7, 23) });
            users.Add(new User() { Id = 2, Name = "Jane Doe", Birthday = new DateTime(1974, 1, 17) });
            users.Add(new User() { Id = 3, Name = "Sammy Doe", Birthday = new DateTime(1991, 9, 2) });

            dgSimple.ItemsSource = users;

            dgSimple.Dispatcher.BeginInvoke(new Action(() =>
            {
                // dgSimple.SetTextTrimming();
            }), DispatcherPriority.Background);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            dgSimple.SetTextTrimming();
            dgSimple.AddCellToolTipWhenTrimming();
        }

        private void DgSimple_OnMouseEnter(object sender, MouseEventArgs e)
        {
            // throw new NotImplementedException();
            Debug.Print($"MouseEnter: {sender.GetType().Name}");
        }
    }
}
