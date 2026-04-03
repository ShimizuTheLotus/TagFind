using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes.DataTypes
{
    public class ReferencedFileInfo
    {
        public long DataItemID { get; set; }
        public string Path { get; set; } = string.Empty;
        public long StorageSize { get; set; }
    }
}
