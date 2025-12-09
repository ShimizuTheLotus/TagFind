using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagFind.Classes.DB;
using TagFind.Interfaces.IPageNavigationParameter;

namespace TagFind.Classes.DataTypes
{
    public class PageNavigateParameter
    {
        public class DataItemExplorerPageNavigationParameter : PageNavigateParameter, IDBContentManagerParameter, ISearchConditionsParameter, IExplorerPathParameter
        {
            public DBContentManager? DBContentManager { get; set; }
            public ObservableCollection<SearchCondition> SearchConditions { get; set; } = [];
            public ObservableCollection<ExplorerFolder> Path { get; set; } = [];
        }

        public class DataItemDetailPageNavigationParameter : PageNavigateParameter, IDBContentManagerParameter, ISearchConditionsParameter, IDataItemParameter, IExplorerPathParameter
        {
            public DBContentManager? DBContentManager { get; set; }
            public ObservableCollection<SearchCondition> SearchConditions { get; set; } = [];
            public DataItem DataItem { get; set; } = new() { ID = -1 };
            public ObservableCollection<ExplorerFolder> Path { get; set; } = [];
        }

        public class DataItemEditPageNavigationParameter : PageNavigateParameter, IDBContentManagerParameter, IDataItemParameter, IExplorerPathParameter
        {
            public DBContentManager? DBContentManager { get; set; }
            public DataItem DataItem { get; set; } = new() { ID = -1 };
            public ObservableCollection<ExplorerFolder> Path { get; set; } = [];
        }

        public class TagEditPageNavigationParameter : PageNavigateParameter, IDBContentManagerParameter, ITagParameter
        {
            public DBContentManager? DBContentManager { get; set; }
            public Tag Tag { get; set; } = new();
        }
    }
}
