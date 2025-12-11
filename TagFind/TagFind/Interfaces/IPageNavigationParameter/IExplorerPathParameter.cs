using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagFind.Classes.DataTypes;

namespace TagFind.Interfaces.IPageNavigationParameter
{
    internal interface IExplorerPathParameter
    {
        public ObservableCollection<ExplorerFolder> Path { get; set; }
    }

    public static class IExplorerPathParameterExtensions
    {
        public static ObservableCollection<ExplorerFolder> Duplicate(this ObservableCollection<ExplorerFolder> stack)
        {
            ObservableCollection<ExplorerFolder> result = [];
            foreach (var item in stack)
            {
                result.Add(item);
            }
            return result;
        }
    }
}
