using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagFind.UI;

namespace TagFind.Classes.DataTypes
{
    public class DataItemSearchConfig
    {
        public SearchModeEnum SearchMode { get; set; } = SearchModeEnum.Global;
        public long ParentOrAncestorIDLimit = -1;
        public bool SearchTitle { get; set; } = true;
        public bool SearchDescription { get; set; } = true;
        public bool SetSearch { get; set; } = true;
        /// <summary>
        /// There's tag A, B, C.
        /// A -Contains-> B information is stored in tag B.
        /// B -Contains-> C information is stored in tag B.
        /// If FindSetStoredIndifferentTags is false, we can't use set search to get tag C, unless tag C have an info A -Contains-> B -Contains-> C.
        /// If its true, we can get tag C.
        /// </summary>
        public bool FindSetStoredIndifferentTags { get; set; } = true;
    }
}
