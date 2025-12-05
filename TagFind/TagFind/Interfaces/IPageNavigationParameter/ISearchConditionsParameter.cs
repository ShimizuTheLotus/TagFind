using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagFind.Classes.DataTypes;

namespace TagFind.Interfaces.IPageNavigationParameter
{
    internal interface ISearchConditionsParameter
    {
        public ObservableCollection<SearchCondition> SearchConditions { get; set; }
    }
}
