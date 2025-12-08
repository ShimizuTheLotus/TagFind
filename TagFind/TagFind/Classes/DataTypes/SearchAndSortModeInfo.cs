using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes.DataTypes
{
    public class SearchAndSortModeInfo
    {
        public SortModeEnum SortMode = SortModeEnum.ID;
        public SortDirectionEnum SortDirection = SortDirectionEnum.ASC;
        public TextMatchModeEnum TextMatchMode = TextMatchModeEnum.AllResults;
    }

    public enum SortModeEnum
    {
        ID,
        Title,
        CreatedTime,
        ModifiedTime
    }

    public enum SortDirectionEnum
    {
        ASC, // Ascending
        DESC // Descending
    }

    public enum TextMatchModeEnum
    {
        Fast, // Use MATCH, the results might be incomplete.
        AllResults // Use LIKE, get all results but might be slow.
    }
}
