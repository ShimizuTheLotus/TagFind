using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Interfaces
{
    internal interface ITagFinddDatabaseOption : IDatabaseOption
    {
        public void AddTag();
        public void RemoveTag();  }
}
