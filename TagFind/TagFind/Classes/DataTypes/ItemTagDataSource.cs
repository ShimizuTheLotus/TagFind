using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TagFind.Classes.Consts.DB.UserDB;

namespace TagFind.Classes.DataTypes
{
    public class ItemTagDataSource
    {
        /// <summary>
        /// Unique ID, only used for edit or delete.
        /// </summary>
        public long ID;
        /// <summary>
        /// Which property this tag used to connect to the item or its parent.
        /// When connected to item, the value was -1.
        /// </summary>
        public long ParentPropertyID = -1;
        public string ParentPropertyName = string.Empty;
        /// <summary>
        /// Which ID attached to the item or its parent.
        /// </summary>
        public long TagID;
        public string TagName = string.Empty;
        /// <summary>
        /// If connected to the item, the value was -1, or it was TagID of the parent tag.
        /// </summary>
        public long ParentTagID = -1;
    }

    public static class ItemTagDataSourceExtensions
    {
        public static void AddItemTag(this List<ItemTagDataSource> ItemTags, ItemTagDataSource tag)
        {
            if (!ItemTags.HasItemTag(tag.TagID))
            {
                ItemTags.Add(tag);
                ItemTags.Sort();
            }
        }

        public static bool HasItemTag(this List<ItemTagDataSource> ItemTags, long tagID)
        {
            return ItemTags.Any(x => x.TagID == tagID);
        }

        public static void RemoveItemTag(this List<ItemTagDataSource> ItemTags, ItemTagDataSource tag)
        {
            ItemTags.RemoveItemTag(tag.TagID);
        }

        public static void RemoveItemTag(this List<ItemTagDataSource> ItemTags, long ID)
        {
            if (ItemTags.Any(x => x.ID == ID))
            {
                ItemTags.Remove(ItemTags.First(x => x.TagID == ID));
            }
        }

        public static List<long> GetPropertyChilds(this List<ItemTagDataSource> ItemTags, long parentTagID, long propertyID)
        {
            return ItemTags.Where(x => x.ParentTagID == parentTagID && x.ParentPropertyID == propertyID)
                .Select(x => x.TagID)
                .ToList();
        }

        public static List<ItemTagDataSource> SortByProperty(this List<ItemTagDataSource> ItemTags)
        {
            List<ItemTagDataSource> result = new();
            Queue<ItemTagDataSource> queue = new();

            // Get root
            IEnumerable<ItemTagDataSource> rootItems = ItemTags.Where(item => item.ParentTagID == -1);
            foreach (ItemTagDataSource root in rootItems)
            {
                queue.Enqueue(root);
            }

            while (queue.Count > 0)
            {
                ItemTagDataSource current = queue.Dequeue();
                result.Add(current);

                IEnumerable<ItemTagDataSource> children = ItemTags.Where(item => item.ParentTagID == current.TagID);
                foreach (ItemTagDataSource child in children)
                {
                    queue.Enqueue(child);
                }
            }

            return result;
        }

        public static List<ItemTagTreeItem> ConvertIntoItemTagTree(this List<ItemTagDataSource> source)
        {
            // Find roots
            List<ItemTagTreeItem> result = [];
            foreach (ItemTagDataSource item in source)
            {
                if (item.ParentTagID == -1)
                {
                    ItemTagTreeItem tagTreeItem = new()
                    {
                        TagID = item.TagID,
                        TagName = item.TagName,
                    };
                    tagTreeItem.PropertyItems = tagTreeItem.GetItemTagTreePropertyChild(source);
                    result.Add(tagTreeItem);
                }
            }
            return result;
        }

        private static List<ItemTagTreePropertyItem> GetItemTagTreePropertyChild(this ItemTagTreeItem item, List<ItemTagDataSource> source)
        {
            List<ItemTagTreePropertyItem> result = [];
            // get child tags.
            List<ItemTagDataSource> children = source.Select(x => x).Where(x => x.ParentTagID == item.TagID).ToList();
            foreach (ItemTagDataSource child in children)
            {
                // Child properties not added, add it.
                if (!result.Any(x => x.PropertyID == child.ParentPropertyID))
                {
                    ItemTagTreePropertyItem propertyItem = new()
                    {
                        PropertyID = child.ParentPropertyID,
                        PropertyName = child.ParentPropertyName,
                    };

                    // Find child of this property.
                    propertyItem.Children = propertyItem.GetItemTagTreeTagChild(source);

                    result.Add(propertyItem);
                }
            }
            return result;
        }

        private static List<ItemTagTreeItem> GetItemTagTreeTagChild(this ItemTagTreePropertyItem item, List<ItemTagDataSource> source)
        {
            List<ItemTagTreeItem> result = [];
            List<ItemTagDataSource> children = source.Select(x => x).Where(x => x.ParentPropertyID == item.PropertyID).ToList();
            foreach (ItemTagDataSource child in children)
            {
                if (!result.Any(x => x.TagID == child.ParentTagID))
                {
                    ItemTagTreeItem tagItem = new()
                    {
                        TagID = child.TagID,
                        TagName = child.TagName,
                    };
                    tagItem.PropertyItems = tagItem.GetItemTagTreePropertyChild(source);
                    result.Add(tagItem);
                }
            }
            return result;
        }
    }
}
