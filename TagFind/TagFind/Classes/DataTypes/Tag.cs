using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes.DataTypes
{
    public class Tag : INotifyPropertyChanged
    {
        public long ID
        {
            get => _id;
            set
            {
                _id = value;
                PropertyChanged?.Invoke(this, new(nameof(ID)));
            }
        }
        private long _id = -1;
        public string MainName
        {
            get => _mainName;
            set
            {
                _mainName = value;
                PropertyChanged?.Invoke(this, new(nameof(MainName)));
            }
        }
        private string _mainName = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                PropertyChanged?.Invoke(this, new(nameof(Description)));
            }
        }
        public string _description = string.Empty;
        public DateTime CreatedTime
        {
            get => _createdTime;
            set
            {
                _createdTime = value;
                PropertyChanged?.Invoke(this, new(nameof(CreatedTime)));
            }
        }
        private DateTime _createdTime = DateTime.UtcNow;
        public DateTime ModifiedTime
        {
            get => _modifiedTime;
            set
            {
                _modifiedTime = value;
                PropertyChanged?.Invoke(this, new(nameof(ModifiedTime)));
            }
        }
        private DateTime _modifiedTime = DateTime.UtcNow;
        public List<string> Surnames
        {
            get => _surnames;
            set
            {
                _surnames = value;
                PropertyChanged?.Invoke(this, new(nameof(Surnames)));
            }
        }
        private List<string> _surnames = [];
        public List<LogicChain> LogicChains
        {
            get => _logicChains;
            set
            {
                _logicChains = value;
                PropertyChanged?.Invoke(this, new(nameof(LogicChains)));
            }
        }
        private List<LogicChain> _logicChains = [];
        public List<PropertyItem> PropertyItems
        {
            get => _propertyItems;
            set
            {
                _propertyItems = value;
                PropertyChanged?.Invoke(this, new(nameof(PropertyItems)));
            }
        }
        private List<PropertyItem> _propertyItems = [];

        /// <summary>
        /// This tag refers to a same entity as the tags in other tag sources in this list.
        /// The tag source name cannot contain ":" character.
        /// The format of each string is "TagSource:TagID".
        /// TagID can be any string.
        /// </summary>
        public List<string> SameTagList
        {
            get => _sameTagList;
            set
            {
                _sameTagList = value;
                PropertyChanged?.Invoke(this, new(nameof(SameTagList)));
            }
        }
        private List<string> _sameTagList = [];

        public Tag(int ID, string MainName)
        {
            this.ID = ID;
            this.MainName = MainName;
        }
        public Tag()
        {

        }
        public event PropertyChangedEventHandler? PropertyChanged;

        public string IconGlyph { get; set; } = "\uE8EC";// The icon for set in TokenizedTextBox, DO NOT EDIT
    }

    public static class TagExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static List<Tag> LogicChainEndWithParentPropertyID(this List<Tag> tags, long propertyID = default)
        {
            if (propertyID == -1)
                return tags;

            List<Tag> targetTags = [];
            foreach (Tag tag in tags)
            {
                foreach (LogicChain chain in tag.LogicChains)
                {
                    if (chain.LogicChainData.Count == 0)
                        continue;
                    // Check if the LogicChainItem ends with the specified ParentPropertyItemID
                    if (chain.LogicChainData[^1].ParentPropertyItemID == propertyID)
                    {
                        targetTags.Add(tag);
                        break;
                    }
                }
            }
            return targetTags;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static List<Tag> LogicChainHaveAncestorWithParentPropertyID(this List<Tag> tags, long propertyID = default)
        {
            if(propertyID == -1)
                return tags;

            List<Tag> targetTags = [];
            foreach (Tag tag in tags)
            {
                foreach (LogicChain chain in tag.LogicChains)
                {
                    foreach (LogicChainItem item in chain.LogicChainData)
                    {
                        // Check if the LogicChainItem have an ancestor with the specified ParentPropertyItemID
                        if (item.ParentPropertyItemID == propertyID)
                        {
                            targetTags.Add(tag);
                            break;
                        }
                    }
                }
            }
            return targetTags;
        }

        public static List<string> SplitIntoSurnames(this string surnamesString)
        {
            List<string> surnames = [];
            foreach (string surname in surnamesString.Trim().Split(Environment.NewLine))
            {
                surnames.Add(surname.Trim('\r').Trim('\n'));
            }
            return surnames;
        }

        public static string CombineSurnames(this List<string> surnames)
        {
            return string.Join(Environment.NewLine, surnames);
        }
    }

    public class TagIDEqualityComparer : IEqualityComparer<Tag>
    {
        public bool Equals(Tag? a, Tag? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.ID == b.ID;
        }
        public int GetHashCode(Tag tag) => HashCode.Combine(tag.ID);
    }
}
