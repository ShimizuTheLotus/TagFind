using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagFind.Classes.DataTypes;

namespace TagFind.Interfaces.IPageNavigationParameter
{
    public interface IDataItemListParameter
    {
        public List<DataItem> DataItemList { get; set; }
    }
}
