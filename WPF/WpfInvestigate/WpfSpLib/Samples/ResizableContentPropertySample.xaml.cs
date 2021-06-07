﻿using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WpfSpLib.Samples
{
    /// <summary>
    /// Interaction logic for ResizableContentPropertySample.xaml
    /// </summary>
    public partial class ResizableContentPropertySample : UserControl
    {
        public Thumb MovingThumb => TestThumb;
        public ResizableContentPropertySample()
        {
            InitializeComponent();
        }
    }
}
