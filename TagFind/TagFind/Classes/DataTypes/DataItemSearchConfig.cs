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
    }
}
