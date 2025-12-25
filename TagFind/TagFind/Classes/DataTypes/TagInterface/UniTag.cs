using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes.DataTypes
{
    // UniTag is a tag that refers to a single unique item and can be used to identify that item across different contexts.
    public class UniTag
    {
        /// <summary>
        /// The Guid was generated when the database created, or signed by the publisher so its not always a Guid.
        /// It can also be string.
        /// </summary>
        public string UniTagSourceGUID { get; set; } = string.Empty;
        /// <summary>
        /// Unique ID is unique in the UniqueUniTagSource context.
        /// </summary>
        public string UniqueID { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the collection of language and main name pairs associated with the entity.
        /// Key: Language (e.g., "en", "fr")
        /// Value: MainNameDescriptionInfo containing MainName and Description
        /// </summary>
        public Dictionary<string, MainNameDescriptionInfo> LanguageMainNamePair { get; set; } = [];
        /// <summary>
        /// This tag refers to a same entity as other tags in this list.
        /// </summary>
        public List<string> SameTagList { get; set; } = [];
    }
    public class MainNameDescriptionInfo
    {
        string MainName { get; set; } = string.Empty;
        string Description { get; set; } = string.Empty;
    }
}
