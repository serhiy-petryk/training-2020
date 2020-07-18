using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ColorInvestigation.Lib;
using ColorInvestigation.Temp;
using ColorInvestigation.Views;

namespace ColorInvestigation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnGrayScaleButtonClick(object sender, RoutedEventArgs e)
        {
            new GrayScale().Show();
        }

        private void OnGrayScaleDiffButtonClick(object sender, RoutedEventArgs e)
        {
            new GrayScaleDiff().Show();
        }

        private void OnCalcButtonClick(object sender, RoutedEventArgs e)
        {
            Class1.Calc();
        }
    }
}
