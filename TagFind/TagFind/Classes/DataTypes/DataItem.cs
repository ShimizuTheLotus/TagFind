using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes.DataTypes
{
    public class DataItem
    {
        public long ID = -1;
        public long ParentID = 0;
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public DateTime ModifiedTime { get; set; } = DateTime.Now;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ItemType = string.Empty;
        public string RefPath { get; set; } = string.Empty;
        public string SearchText = string.Empty;
        public List<ItemTagTreeItem> ItemTags = [];
        public DataItem(string Title, string Description, string SearchText)
        {
            this.Title = Title;
            this.Description = Description;
            this.SearchText = SearchText;
        }
        public DataItem()
        {

        }
    }

    public enum SearchModeEnum
    {
        Global,
        Folder,
        Layer
    }

    public class DataItemEqualityComparer : IEqualityComparer<DataItem>
    {
        public bool Equals(DataItem? a, DataItem? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.ID == b.ID;
        }
        public int GetHashCode(DataItem a) => HashCode.Combine(a.ID);
    }
}
