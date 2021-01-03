﻿using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WpfInvestigate.Samples
{
    /// <summary>
    /// Interaction logic for ResizableContentPropertySample.xaml
    /// </summary>
    public partial class ResizableContentPropertySample : UserControl
    {
        public Thumb DragThumb => TestThumb;
        public ResizableContentPropertySample()
        {
            InitializeComponent();
        }
    }
}
