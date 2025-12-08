using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes
{
    public class Consts
    {
        public class DB
        {
            public class DBListDB
            {
                
            }

            public class UserDB
            {
                public class Meta
                {
                    public string Property = nameof(Property);
                    public string Value = nameof(Value);
                    
                    // Property names
                    public string Description = nameof(Description);
                    public string Version = nameof(Version);
                }

                public class TagPool
                {
                    public string ID = nameof(ID);
                    public string MainName = nameof(MainName);
                    public string Description = nameof(Description);
                    public string CreatedTime = nameof(CreatedTime);
                    public string ModifiedTime = nameof(ModifiedTime);
                    
                    // External:
                    //public string SurNames = nameof(SurNames);
                    //public string LogicChains = nameof(LogicChains);
                    //public string FeatureItems = nameof(FeatureItems);
                }

                public class TagData
                {
                    public string ID = nameof(ID);
                    public string TagID = nameof(TagID);
                    public string Seq = nameof(Seq);
                    public string Type = nameof(Type);
                    public string Value = nameof(Value);

                    // Types
                    public string Surname = nameof(Surname);
                    public string LogicChainItem = nameof(LogicChainItem);
                    public string PropertyItem = nameof(PropertyItem);
                    public string RestrictionLogicChainItem = nameof(RestrictionLogicChainItem);
                }

                public class PropertyTemplates
                {
                    public string ID = nameof(ID);
                    public string TemplateName = nameof(TemplateName);
                    public string PropertyName = nameof(PropertyName);
                    public string Seq = nameof(Seq);
                    public string TagParentRestriction = nameof(TagParentRestriction);
                }

                public class DataItems
                {
                    public string ID = nameof(ID);
                    public string ParentItemID = nameof(ParentItemID);
                    //public string Title = nameof(Title);
                    //public string Description = nameof(Description);
                    public string Type = nameof(Type);
                    public string CreatedTime = nameof(CreatedTime);
                    public string ModifiedTime = nameof(ModifiedTime);
                    //public string RefPath = nameof(RefPath);
                    //public string SearchText = nameof(SearchText);
                }

                public class DataItemFastSearch
                {
                    public string DataItemID = nameof(DataItemID);
                    public string Title = nameof(Title);
                    public string Description = nameof(Description);
                    public string RefPath = nameof(RefPath);
                    public string SearchText = nameof(SearchText);
                }

                public class ItemTags
                {
                    public string ItemID = nameof(ItemID);
                    public string PropertyID = nameof(PropertyID);
                    public string TagID = nameof(TagID);
                    public string ParentTagID = nameof(ParentTagID);
                }
            }
        }

        public static class ConnectedAnimationKeys
        {
            public static object OpenDatabaseFromListAnimation = new();
            public static object DBContentDataItemViewDetailAnimation = new();
            public static object DBContentDataItemEnterEditModePage = new();
        }

        public static class ResourceKeys
        {
            public static class ThemeResourceKeys
            {
                public static object SystemControlForegroundBaseHighBrush = new();
                public static object TextBoxForegroundThemeBrush = new();
            }
        }
    }
}
