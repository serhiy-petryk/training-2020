﻿using System.Windows;
using WpfInvestigate.Helpers;

namespace WpfInvestigate.TestViews
{
    /// <summary>
    /// Interaction logic for MemoryLeakInvestigation.xaml
    /// </summary>
    public partial class MemoryLeakInvestigation : Window
    {
        public MemoryLeakInvestigation()
        {
            InitializeComponent();
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            // var a1 = System.Windows.Data.BindingOperations.GetBinding(Button, ContentProperty);
            // BindingOperations.ClearAllBindings(Button);
            // BindingOperations.ClearAllBindings(MwiBootstrapColorTests.Instance);
            // Tips.ClearAllBindings(Button);
            if (MwiBootstrapColorTests.Instance != null)
                BindingHelper.ClearAllBindings(MwiBootstrapColorTests.Instance);
        }
    }
}
