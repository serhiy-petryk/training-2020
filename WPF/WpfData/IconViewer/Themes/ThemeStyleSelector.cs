using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

namespace IconViewer.Themes
{
  public class ThemeStyleSelector : StyleSelector
  {
    public override Style SelectStyle(object item, DependencyObject container)
    {
      var th = (ThemeInfo)item;
      if (th.Resource == null)
        return null;

      // return th.Resource[typeof(ComboBoxItem)] as Style;
      var a1 = th.Resource[typeof(ToolBar)] as Style;
      //var a2 = th.Resource[typeof(ToolBarBu)] as Style;

      string gridXaml = XamlWriter.Save(th.Resource[typeof(ComboBoxItem)] as Style);
      StringReader stringReader = new StringReader(gridXaml);
      XmlReader xmlReader = XmlReader.Create(stringReader);
      Style s1 = (Style)XamlReader.Load(xmlReader);

      Style s2 = new Style();
      // s2.Setters.Add(new Setter(DependencyProperty.for, ));
      var cbi = container as ComboBoxItem;
      // cbi.Style = s1;
      Setter aa1 = a1.Setters[0] as Setter;
      cbi.Background = aa1.Value as Brush;
      // cbi.Background = new SolidColorBrush(Colors.Red);
      return s1;
    }
  }
}
