using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace TagFind.Classes.DataTypes
{
    public class SearchCondition 
    {
        public class TextCondition : SearchCondition
        {
            public string MainName { get; set; } = string.Empty;
            public string IconGlyph { get; set; } = "\uE8D2";// The icon for set in TokenizedTextBox, DO NOT EDIT
        }

        public class TagCondition : SearchCondition
        {
            public long TagID { get; set; } = -1;
            public string TagName { get; set; } = string.Empty;

            public string MainName { get => TagName; set => TagName = value; }
            public string IconGlyph { get; set; } = "\uE8EC";// The icon for set in TokenizedTextBox, DO NOT EDIT
        }
    }

    public static class SearchConditionExtensions
    {
        public static ObservableCollection<SearchCondition> Duplicate(this ObservableCollection<SearchCondition> source)
        {
            ObservableCollection<SearchCondition> conditions = [];
            foreach (var i in source)
            {
                conditions.Add(i);
            }
            return conditions;
        }
    }
}
