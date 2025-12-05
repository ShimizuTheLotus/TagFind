using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Interfaces
{
    public interface IDatabaseListInfo
    {
        public int ID { get; set; }
        public string Path { get; set; }
        public bool IsValid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
