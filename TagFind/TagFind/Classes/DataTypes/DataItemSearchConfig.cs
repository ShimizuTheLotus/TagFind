using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes.DataTypes
{
    public class DataItemSearchConfig
    {
        public long ParentIDLimit = -1;
        public bool SearchTitle { get; set; } = true;
        public bool SearchDescription { get; set; } = true;
    }
}
