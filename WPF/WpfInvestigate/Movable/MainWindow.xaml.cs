﻿using System.Windows;
using WpfInvestigate.Samples;

namespace Movable
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

        private void AddChildToCanvas_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = new SampleDialogMovable();
            Canvas.Children.Add(a1);
        }

        private void AddChildToGrid_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = new SampleDialogMovable();
            Grid.Children.Add(a1);
        }

        private void AddChildToItemsControl_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = new SampleDialogMovable();
            ItemsControl.Items.Add(a1);
        }

        private void AddChildToItemsControlGrid_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = new SampleDialogMovable();
            ItemsControlGrid.Items.Add(a1);
        }

        private void AddChildToItemsControlCanvas_OnClick(object sender, RoutedEventArgs e)
        {
            var a1 = new SampleDialogMovable();
            ItemsControlCanvas.Items.Add(a1);
        }
    }
}
