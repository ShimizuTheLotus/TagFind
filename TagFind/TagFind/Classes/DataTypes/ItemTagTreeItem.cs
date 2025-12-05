using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes.DataTypes
{
    public class ItemTagTreeItem
    {
        public long TagID { get; set; } = -1;
        public string TagName { get; set; } = string.Empty;
        public List<ItemTagTreePropertyItem> PropertyItems { get; set; } = [];
    }

    public class ItemTagTreePropertyItem
    {
        public long PropertyID { get; set; } = -1;
        public string PropertyName { get; set; } = string.Empty;
        public List<ItemTagTreeItem> Children { get; set; } = [];
    }

    public static class ItemTagTreeItemExtensions
    {
        public static List<ItemTagDataSource> ConvertIntoItemTagDataSource(this List<ItemTagTreeItem> source)
        {
            List<ItemTagDataSource> result = new();

            foreach (ItemTagTreeItem treeItem in source)
            {
                ConvertTagTreeItemRecursive(treeItem, -1, string.Empty, result);
            }

            return result;
        }

        private static void ConvertTagTreeItemRecursive(ItemTagTreeItem tagItem, long parentTagId, string parentPropertyName, List<ItemTagDataSource> result)
        {
            ItemTagDataSource dataSource = new()
            {
                ID = -1,
                ParentPropertyID = -1,
                ParentPropertyName = parentPropertyName,
                TagID = tagItem.TagID,
                TagName = tagItem.TagName,
                ParentTagID = parentTagId
            };

            result.Add(dataSource);

            foreach (ItemTagTreePropertyItem propertyItem in tagItem.PropertyItems)
            {
                ConvertPropertyTreeItemRecursive(propertyItem, tagItem.TagID, result);
            }
        }

        private static void ConvertPropertyTreeItemRecursive(ItemTagTreePropertyItem propertyItem, long parentTagId, List<ItemTagDataSource> result)
        {
            foreach (ItemTagTreeItem childTag in propertyItem.Children)
            {
                ConvertTagTreeItemRecursive(childTag, parentTagId, propertyItem.PropertyName, result);
            }
        }
    }
}
