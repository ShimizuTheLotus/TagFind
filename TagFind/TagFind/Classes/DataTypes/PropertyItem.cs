using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes.DataTypes
{
    public class PropertyItem
    {
        public long ID { get; set; }
        public long TagID { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public long Seq { get; set; }
        /// <summary>
        /// Is "contains" relation means the child tag using this property to attach the parent tag in its logic chains, the child tag is a subset of the parent tag.
        /// When "set" search mode was enabled, the search results can include subsets of the searched tag.
        /// </summary>
        public bool IsContainsRelation { get; set; } = false;
        public List<LogicChain> RestrictedTagLogicChains { get; set; } = [];

        public PropertyItem()
        {

        }
    }

    public static class PropertyItemExtensions
    {
        public static bool ContainsPropertyItem(this List<PropertyItem> list, PropertyItem item)
        {
            if (item.ID == -1) return false;
            foreach (PropertyItem prop in list)
            {
                if (prop.ID == item.ID)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
