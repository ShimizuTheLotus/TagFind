using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagFind.Classes.DataTypes;

namespace TagFind.Interfaces
{
    internal interface IDatabaseSearchConditionEditablePage
    {
        public ObservableCollection<SearchCondition> searchConditions { get; set; }
        public void AddConditionAndNavigateToExplorerPage(SearchCondition condition);
    }
}
