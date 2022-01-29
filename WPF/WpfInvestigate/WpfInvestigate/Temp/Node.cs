using System.Collections.ObjectModel;

namespace WpfInvestigate.Temp
{
    public class Node
    {
        public string Name { get; set; }
        public ObservableCollection<Node> Children { get; set; }// = new ObservableCollection<Node>();
        public bool IsSortingLabelVisible => Children != null;
//        public override string ToString() => Name;
    }


}
