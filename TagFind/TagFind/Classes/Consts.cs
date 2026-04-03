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
                    public static string Property = nameof(Property);
                    public static string Value = nameof(Value);

                    // Property names
                    public static string Description = nameof(Description);
                    public static string Version = nameof(Version);
                }

                public class TagPool
                {
                    public static string ID = nameof(ID);
                    public static string MainName = nameof(MainName);
                    public static string Description = nameof(Description);
                    public static string CreatedTime = nameof(CreatedTime);
                    public static string ModifiedTime = nameof(ModifiedTime);
                    
                    // External:
                    //public string SurNames = nameof(SurNames);
                    //public string LogicChains = nameof(LogicChains);
                    //public string FeatureItems = nameof(FeatureItems);
                }

                public class TagData
                {
                    public static string ID = nameof(ID);
                    public static string TagID = nameof(TagID);
                    public static string Seq = nameof(Seq);
                    public static string Type = nameof(Type);
                    public static string Value = nameof(Value);

                    // Types
                    public static string Surname = nameof(Surname);
                    public static string LogicChainItem = nameof(LogicChainItem);
                    public static string PropertyItem = nameof(PropertyItem);
                    public static string RestrictionLogicChainItem = nameof(RestrictionLogicChainItem);
                }

                public class PropertyTemplates
                {
                    public static string ID = nameof(ID);
                    public static string TemplateName = nameof(TemplateName);
                    public static string PropertyName = nameof(PropertyName);
                    public static string Seq = nameof(Seq);
                    public static string TagParentRestriction = nameof(TagParentRestriction);
                }

                public class DataItems
                {
                    public static string ID = nameof(ID);
                    public static string ParentItemID = nameof(ParentItemID);
                    //public string Title = nameof(Title);
                    //public string Description = nameof(Description);
                    public static string Type = nameof(Type);
                    public static string CreatedTime = nameof(CreatedTime);
                    public static string ModifiedTime = nameof(ModifiedTime);
                    public static string Title = nameof(Title);
                    //public string RefPath = nameof(RefPath);
                    //public string SearchText = nameof(SearchText);
                }

                public class DataItemFastSearch
                {
                    public static string DataItemID = nameof(DataItemID);
                    public static string Title = nameof(Title);
                    public static string Description = nameof(Description);
                    public static string RefPath = nameof(RefPath);
                    public static string SearchText = nameof(SearchText);
                }

                public class ItemTags
                {
                    public static string ItemID = nameof(ItemID);
                    public static string PropertyID = nameof(PropertyID);
                    public static string TagID = nameof(TagID);
                    public static string ParentTagID = nameof(ParentTagID);
                }

                public class TagCompatibilityTable
                {
                    public static string LocalTagID = nameof(LocalTagID);
                    public static string SourceGUID = nameof(SourceGUID);
                    public static string SourceTagID = nameof(SourceTagID);
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
