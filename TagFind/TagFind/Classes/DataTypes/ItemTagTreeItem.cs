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
        public bool MarkedToDelete { get; set; } = false;
    }

    public class ItemTagTreePropertyItem
    {
        public long PropertyID { get; set; } = -1;
        public string PropertyName { get; set; } = string.Empty;
        public List<ItemTagTreeItem> Children { get; set; } = [];
    }

    public static class ItemTagTreeItemExtensions
    {
        public static void AddTagsToDataItem(this DataItem dataItem, List<ItemTagTreeItem> itemTagTreeItems)
        {
            foreach (ItemTagTreeItem itemTagTreeItem in itemTagTreeItems)
            {
                ItemTagTreeItem? getTag = dataItem.GetDataItemItemsTagTreeItemByID(itemTagTreeItem.TagID);
                if (getTag != null)
                {
                    // Already exists, examine if child items are exist. If not, add it.
                    getTag.AddTagsToDataItemRecursive(itemTagTreeItem);
                }
                // No such a tag in data item.
                else
                {
                    dataItem.ItemTags.Add(itemTagTreeItem);
                }
            }
        }

        private static void AddTagsToDataItemRecursive(this ItemTagTreeItem targetTagTreeItem, ItemTagTreeItem sourceTagTreeItem)
        {
            foreach (ItemTagTreePropertyItem itemTagTreePropertyItem in sourceTagTreeItem.PropertyItems)
            {
                if (targetTagTreeItem.PropertyItems.Any(x => x.PropertyID == itemTagTreePropertyItem.PropertyID))
                {
                    // Try find tag in property's branches.
                    ItemTagTreePropertyItem property = targetTagTreeItem.PropertyItems.First(x => x.PropertyID == itemTagTreePropertyItem.PropertyID);
                    foreach (ItemTagTreeItem tagTreeItem in itemTagTreePropertyItem.Children)
                    {
                        if (property.Children.Any(x => x.TagID == tagTreeItem.TagID))
                        {
                            property.Children.First(x => x.TagID == tagTreeItem.TagID).AddTagsToDataItemRecursive(tagTreeItem);
                        }
                        // Add branch.
                        else
                        {
                            property.Children.Add(tagTreeItem);
                        }
                    }
                }
                // Add this branch.
                else
                {
                    targetTagTreeItem.PropertyItems.Add(itemTagTreePropertyItem);
                }
            }
        }

        public static bool IsBranchEnd(this ItemTagTreeItem item)
        {
            foreach (ItemTagTreePropertyItem itemTagTreePropertyItem in item.PropertyItems)
            {
                if (itemTagTreePropertyItem.Children.Count > 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Remove tags from a data item. If there's some sub items, we need to remove them from specific struct.
        /// SUch as we have Apple -Color-> Red to remove,
        /// we only remove the Red tag from Apple tag if they were connected with the property Color.
        /// </summary>
        /// <param name="dataItem"></param>
        /// <param name="itemTagTreeItemsToBeDeleted"></param>
        public static void MarkToRemoveTagsFromDataItem(this DataItem dataItem, List<ItemTagTreeItem> itemTagTreeItemsToBeDeleted)
        {
            // Find all items matches head node.
            foreach (ItemTagTreeItem itemTagTreeItem in itemTagTreeItemsToBeDeleted)
            {
                // Items match the head node
                List<ItemTagTreeItem> itemsMatchHeadNode = dataItem.GetAllDataItemItemsTagTreeItemByID(itemTagTreeItem.TagID);
                foreach (ItemTagTreeItem headMatchTagTreeItem in itemsMatchHeadNode)
                {
                    headMatchTagTreeItem.MarkToRemoveTagsAtTargetNodeRecursive(itemTagTreeItem);
                }
            }
        }

        public static void MarkToRemoveTagsAtTargetNodeRecursive(this ItemTagTreeItem targetTagTreeItem, ItemTagTreeItem currentLayerTreeItem)
        {
            // This means the currentLayerTreeItem is what we're going to delete.
            if (currentLayerTreeItem.IsBranchEnd())
            {
                if (targetTagTreeItem.TagID == currentLayerTreeItem.TagID)
                {
                    targetTagTreeItem.MarkedToDelete = true;
                }
                return;
            }

            // No next node.
            if (targetTagTreeItem.IsBranchEnd()) return;

            // Try find next node.
            foreach (ItemTagTreePropertyItem targetItemTagTreePropertyItem in targetTagTreeItem.PropertyItems)
            {
                if (currentLayerTreeItem.PropertyItems.Any(x => x.PropertyID == targetItemTagTreePropertyItem.PropertyID && x.Children.Count > 0))
                {
                    List<ItemTagTreePropertyItem> itemTagTreePropertyItems = currentLayerTreeItem.PropertyItems.Where(x => x.PropertyID == targetItemTagTreePropertyItem.PropertyID && x.Children.Count > 0).ToList();
                    foreach (ItemTagTreeItem nextLayerTargetItemTagTreeItem in targetItemTagTreePropertyItem.Children)
                    {
                        foreach (ItemTagTreePropertyItem nextLayerTagTreePropertyItem in itemTagTreePropertyItems)
                        {
                            foreach (ItemTagTreeItem nextLayerTagTreeItem in nextLayerTagTreePropertyItem.Children)
                            {
                                nextLayerTargetItemTagTreeItem.MarkToRemoveTagsAtTargetNodeRecursive(nextLayerTagTreeItem);
                            }
                        }
                    }
                }
            }
        }

        public static List<ItemTagTreeItem> GetAllDataItemItemsTagTreeItemByID(this DataItem dataItem, long targetID)
        {
            List<ItemTagTreeItem> results = [];
            foreach (ItemTagTreeItem itemTagTreeItem in dataItem.ItemTags)
            {
                if (itemTagTreeItem.TagID == targetID)
                {
                    results.Add(itemTagTreeItem);
                }
                itemTagTreeItem.GetAllDataItemTagTreeItemRecursive(targetID, results);
            }
            return results;
        }

        private static void GetAllDataItemTagTreeItemRecursive(this ItemTagTreeItem treeItem, long targetID, List<ItemTagTreeItem> results)
        {
            foreach (ItemTagTreePropertyItem itemTagTreePropertyItem in treeItem.PropertyItems)
            {
                foreach (ItemTagTreeItem itemTagTreeItem in itemTagTreePropertyItem.Children)
                {
                    if (itemTagTreeItem.TagID == targetID)
                    {
                        results.Add(itemTagTreeItem);
                    }
                    itemTagTreeItem.GetAllDataItemTagTreeItemRecursive(targetID, results);
                }
            }
        }

        public static ItemTagTreeItem? GetDataItemItemsTagTreeItemByID(this DataItem dataItem, long targetID)
        {
            ItemTagTreeItem? result = null;
            foreach (ItemTagTreeItem tagTreeItem in dataItem.ItemTags)
            {
                result = GetItemTagTreeItemRecursiveByID(tagTreeItem, targetID);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private static ItemTagTreeItem? GetItemTagTreeItemRecursiveByID(ItemTagTreeItem itemTagTreeItem, long targetID)
        {
            if (itemTagTreeItem.TagID == targetID)
            {
                return itemTagTreeItem;
            }
            foreach (ItemTagTreePropertyItem propertyItem in itemTagTreeItem.PropertyItems)
            {
                foreach (ItemTagTreeItem tagTreeItem in propertyItem.Children)
                {
                    ItemTagTreeItem? result = GetItemTagTreeItemRecursiveByID(tagTreeItem, targetID);
                    if (result != null) return result;
                }
            }
            return null;
        }

        public static List<ItemTagDataSource> ConvertIntoItemTagDataSource(this List<ItemTagTreeItem> source)
        {
            List<ItemTagDataSource> result = [];

            // Root node
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
