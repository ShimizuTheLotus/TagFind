using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagFind.Interfaces;

namespace TagFind.Classes.DB
{
    public class MetaData : IDatabaseListInfo
    {
        public int ID { get; set; }
        public string Path { get; set; } = string.Empty; 
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}