using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;

namespace WpfInvestigate.Temp.TreeViewClasses
{
    public class PropertyItem
    {
        private static int cnt;
        public int Id { get; } = cnt++;
        public string Name { get; set; }

        public PropertyItem(string name)
        {
            Name = name;
        }
    }

    public class PropertyGroupItem
    {
        public PropertyGroupItem Parent { get; private set; }
        public PropertyItem Item { get; private set; }
        public ListSortDirection SortDirection { get; private set; }
        public string Name => Item == null ? "Sortings:" : Item.Name;
        public string Type => Parent == null ? "Root" : (Item == null ? "Label" : ( Parent.Type == "Root" ? "Group" : "Sort"));
        public ObservableCollection<PropertyGroupItem> Children { get; } = new ObservableCollection<PropertyGroupItem>();
        public PropertyGroupItem Root => Parent == null ? this : Parent.Root;
        public System.Windows.Media.Color BaseColor
        {
            get
            {
                var groupItem = Type == "Group" ? this : Parent;
                var groupIndex = Root.Children.IndexOf(groupItem) + 1;
                var groupColor = Color.GroupColors[groupIndex];
                return System.Windows.Media.Color.FromArgb(255, groupColor.R, groupColor.G, groupColor.B);
            }
        }

        public PropertyGroupItem AddItem(PropertyItem item, ListSortDirection sortDirection)
        {
            var newItem = new PropertyGroupItem {Parent = this, Item = item, SortDirection = sortDirection};
            Children.Add(newItem);
            if (newItem.Type == "Group")
                newItem.Children.Add(new PropertyGroupItem { Parent = newItem }); // Add "Sortings:" label
            return newItem;
        }
    }

    public class SortingItemModel
    {
        public int Level { get; set; } = 0;
        public PropertyItem Item { get; set; }
        public ListSortDirection SortDirection { get; set; }
        public string Name => Item == null ? "Sortings:" : Item.Name;
        public virtual string Type => Item == null ? "Label" : "Sort";
        public ObservableCollection<SortingItemModel> Children { get; } = new ObservableCollection<SortingItemModel>();
        public virtual System.Windows.Media.Color BaseColor => Colors.Transparent;

        public SortingItemModel(PropertyItem item, ListSortDirection sortDirection)
        {
            Item = item;
            SortDirection = sortDirection;
        }
        public bool IsLabel => Item == null;
        public override string ToString() => Name;
    }

    public class GroupingItemModel : SortingItemModel
    {
        public override string Type => "Group";
        public override System.Windows.Media.Color BaseColor
        {
            get
            {
                var groupColor = Color.GroupColors[Level + 1];
                return System.Windows.Media.Color.FromArgb(255, groupColor.R, groupColor.G, groupColor.B);
            }
        }

        public GroupingItemModel(PropertyItem item, ListSortDirection sortDirection, IEnumerable<SortingItemModel> sortings): base(item, sortDirection)
        {
            Children.Add(new SortingItemModel(null, ListSortDirection.Ascending)); // Add label
            if (sortings != null)
            {
                foreach (var sortingItem in sortings)
                    Children.Add(sortingItem);
            }
        }

        public void AddGroup(GroupingItemModel groupItem)
        {
            groupItem.Level = Level + 1;
            Children.Add(groupItem);
        }
    }



}
