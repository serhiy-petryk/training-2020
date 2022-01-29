using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

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

    public class SortingItemModel
    {
        public PropertyItem Item { get; set; }
        public ListSortDirection SortDirection { get; set; }
        public string Name => Item == null ? "Sortings:" : Item.Name;
        public virtual string Type => Item == null ? "Label" : "Sort";
        public ObservableCollection<SortingItemModel> Children { get; } = new ObservableCollection<SortingItemModel>();

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
            Children.Add(groupItem);
        }
    }



}
