using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagFind.Classes.DB;

namespace TagFind.Interfaces
{
    public interface IDBContentAccessiblePage
    {
        public DBContentManager ContentManager { get; set; }
    }
}
