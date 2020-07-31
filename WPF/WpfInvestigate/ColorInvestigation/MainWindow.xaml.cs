using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup.Primitives;
using System.Windows.Media;
using ColorInvestigation.Lib;
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

        private void OnTestButtonClick(object sender, RoutedEventArgs e)
        {
            ColorSpacesTests.HueTest();
            ColorSpacesTests.HslTest();
            ColorSpacesTests.HsvTest();

            var c21 = Color.FromRgb(169, 104, 54);
            var c22 = ColorUtilities.ColorToYCbCr(c21, ColorUtilities.YCbCrType.BT601);
            var c23 = ColorUtilities.YCbCrToColor(c22.Item1, c22.Item2, c22.Item3, ColorUtilities.YCbCrType.BT601);
            ColorSpacesTests.YCbCrTest(ColorUtilities.YCbCrType.BT601);
            ColorSpacesTests.YCbCrTest(ColorUtilities.YCbCrType.BT709);
            ColorSpacesTests.YCbCrTest(ColorUtilities.YCbCrType.BT2020);


            var a1 = ColorUtilities.ColorToXyz(Colors.White);
            var a2 = ColorUtilities.XyzToColor(a1.Item1, a1.Item2, a1.Item3);
            var a3 = ColorUtilities.XyzToColor(95.047, 100.000, 108.883);
            var a31 = ColorUtilities.XyzToColor(95, 100.000, 109);

            //ColorXyz.Test(); // OK! 
            // ColorLab.Test(); // OK! 
        }

        private bool IsEqual(double d1, double d2) => Math.Abs(d1 - d2) < 0.0001;

        private void OnGrayScaleButtonClick(object sender, RoutedEventArgs e) => new GrayScale().Show();
        private void OnGrayScaleDiffButtonClick(object sender, RoutedEventArgs e) => new GrayScaleDiff().Show();
        private void OnCalcButtonClick(object sender, RoutedEventArgs e) => Temp.Calc.Calculate();
        private void OnColorSpacesButtonClick(object sender, RoutedEventArgs e) => new ColorSpaces().Show();
        private void OnForegroundButtonClick(object sender, RoutedEventArgs e) => new Foreground().Show();
        private void OnForegroundDiffButtonClick(object sender, RoutedEventArgs e) => new ForegroundDiff().Show();
        private void OnMonoChromaticButtonClick(object sender, RoutedEventArgs e) => new MonoChromatic().Show();

        private void OnHslMonoStyleButtonClick(object sender, RoutedEventArgs e) => new HslMonoStyle().Show();

        private void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            // (FindResource("documentTemplates") as ObjectDataProvider).Refresh();
            var a1 = FindResource("HueAndSaturation");
            var a2 = FindResource("HueAndSaturation") as ObjectDataProvider;
            foreach (BindingExpressionBase be in BindingOperations.GetSourceUpdatingBindings(this))
            {
                be.UpdateSource();
                be.UpdateTarget();
            }

            var bindingList = new List<BindingExpressionBase>();
            GetBindingExpressionsRecursive(this,bindingList);
            foreach (var a3 in bindingList)
            {
                // a3.RelativeSource.
                a3.UpdateSource();
                a3.UpdateTarget();
                // a3.ValidateWithoutUpdate();
            }
        }

        void GetBindingExpressionsRecursive(DependencyObject dObj, List<BindingExpressionBase> bindingList)
        {
            bindingList.AddRange(GetBindingExpressions(dObj));

            int childrenCount = VisualTreeHelper.GetChildrenCount(dObj);
            if (childrenCount > 0)
            {
                for (int i = 0; i < childrenCount; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(dObj, i);
                    GetBindingExpressionsRecursive(child, bindingList);
                }
            }
        }

        void GetBindingsRecursive(DependencyObject dObj, List<BindingBase> bindingList)
        {
            bindingList.AddRange(GetBindingObjects(dObj));

            int childrenCount = VisualTreeHelper.GetChildrenCount(dObj);
            if (childrenCount > 0)
            {
                for (int i = 0; i < childrenCount; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(dObj, i);
                    GetBindingsRecursive(child, bindingList);
                }
            }
        }

        //=================================================
        public static List<BindingExpressionBase> GetBindingExpressions(DependencyObject element)
        {
            var bindings = new List<BindingExpressionBase>();
            List<DependencyProperty> dpList = new List<DependencyProperty>();
            dpList.AddRange(GetDependencyProperties(element));
            dpList.AddRange(GetAttachedProperties(element));

            foreach (DependencyProperty dp in dpList)
            {
                var b = BindingOperations.GetBindingExpressionBase(element, dp);
                if (b != null)
                {
                    bindings.Add(b);
                }
            }

            return bindings;
        }

        public static List<BindingBase> GetBindingObjects(Object element)
        {
            List<BindingBase> bindings = new List<BindingBase>();
            List<DependencyProperty> dpList = new List<DependencyProperty>();
            dpList.AddRange(GetDependencyProperties(element));
            dpList.AddRange(GetAttachedProperties(element));

            foreach (DependencyProperty dp in dpList)
            {
                BindingBase b = BindingOperations.GetBindingBase(element as DependencyObject, dp);
                if (b != null)
                {
                    bindings.Add(b);
                }
            }

            return bindings;
        }

        public static List<DependencyProperty> GetDependencyProperties(object element)
        {
            var markupObject = MarkupWriter.GetMarkupObjectFor(element);
            return markupObject.Properties.Where(mp => mp.DependencyProperty != null).Select(mp => mp.DependencyProperty).ToList();
        }

        public static List<DependencyProperty> GetAttachedProperties(Object element)
        {
            List<DependencyProperty> attachedProperties = new List<DependencyProperty>();
            MarkupObject markupObject = MarkupWriter.GetMarkupObjectFor(element);
            if (markupObject != null)
            {
                foreach (MarkupProperty mp in markupObject.Properties)
                {
                    if (mp.IsAttached)
                    {
                        attachedProperties.Add(mp.DependencyProperty);
                    }
                }
            }

            return attachedProperties;
        }
    }
}
