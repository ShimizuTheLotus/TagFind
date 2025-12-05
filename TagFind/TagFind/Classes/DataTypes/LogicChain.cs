using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes.DataTypes
{
    public class LogicChain : INotifyPropertyChanged
    {
        public long ChainID
        {
            get => _chainID;
            set
            {
                if (_chainID != value)
                {
                    _chainID = value;
                    PropertyChanged?.Invoke(this, new(nameof(ChainID)));
                }
            }
        }
        private long _chainID;
        public List<LogicChainItem> LogicChainData
        {
            get => _logicChainData;
            set
            {
                _logicChainData = value;
                PropertyChanged?.Invoke(this, new(nameof(LogicChainData)));
            }
        }
        private List<LogicChainItem> _logicChainData { get; set; } = [];

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public static class LogicChainExtensions
    {
        public static bool ContainsPath(this LogicChain targetChain, List<LogicChainItem> matchChain)
        {
            if (matchChain.Count == 0)
            {
                return true;
            }

            int startIndex = -1;
            startIndex = targetChain.LogicChainData.FindIndex(x =>
            x.OnChainTagID == matchChain[0].OnChainTagID);
            if (startIndex == -1) return false;

            foreach (LogicChainItem item in matchChain)
            {
                // Out of range.
                if (startIndex >= targetChain.LogicChainData.Count)
                {
                    return false;
                }
                if (targetChain.LogicChainData[startIndex].ParentPropertyItemID != item.ParentPropertyItemID)
                {
                    return false;
                }
                if (item.OnChainTagID == -1 || targetChain.LogicChainData[startIndex].OnChainTagID == -1)
                {
                    // Wildcard match, skip comparison.
                    startIndex++;
                    continue;
                }
                // Item match fail.
                startIndex++;
            }

            return true;
        }

        public static bool ContainsPath(this List<LogicChain> targetChains, List<LogicChainItem> matchChain)
        {
            if (matchChain.Count == 0) return true;
            foreach (LogicChain chain in targetChains)
            {
                if (chain.ContainsPath(matchChain))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ContainsPath(this List<LogicChain> targetChains, List<LogicChain> matchChains)
        {
            if (matchChains.Count == 0) return true;
            foreach(LogicChain matchChain in matchChains)
            {
                if (targetChains.ContainsPath(matchChain.LogicChainData))
                {
                    return true;
                }
            }
            return false;
        }

        public static List<LogicChain> SortLogicChainsRawData(this List<LogicChainItem> logicChainsSourceData)
        {
            List<LogicChain> targetLogicChain = [];
            // Divide values into different LogicChains
            Dictionary<long, List<LogicChainItem>> chainSortDict = [];
            foreach (LogicChainItem item in logicChainsSourceData)
            {
                if (!chainSortDict.ContainsKey(item.ChainID))
                {
                    chainSortDict.Add(item.ChainID, []);
                }
                chainSortDict[item.ChainID].Add(item);
            }

            foreach (KeyValuePair<long, List<LogicChainItem>> rawValue in chainSortDict)
            {
                // One chain per loop
                List<LogicChainItem> chainSource = rawValue.Value;
                // Add LogicChain
                LogicChain? chain = chainSource.SortSingleLogicChainRawData();
                if (chain != null)
                {
                    targetLogicChain.Add(chain);
                }
            }
            return targetLogicChain;
        }

        public static LogicChain? SortSingleLogicChainRawData(this List<LogicChainItem> logicChainSourceData)
        {
            LogicChain chain = new();
            chain.ChainID = logicChainSourceData[0].ChainID;
            LogicChainItem item = new();
            // Find if there's a first LogicChainItem
            if (!logicChainSourceData.Any(x => x.ParentDataItemID == -1))
            {
                return null;
            }
            item = logicChainSourceData.First(x => x.ParentDataItemID == -1);
            logicChainSourceData.Remove(item);
            chain.LogicChainData.Add(item);

            long thisChainItemID = item.ID;
            while (true)
            {
                // Connect chain items.
                if (logicChainSourceData.Count == 0)
                {
                    break;
                }
                if (!logicChainSourceData.Any(x => x.ParentDataItemID == thisChainItemID))
                {
                    break;
                }
                item = logicChainSourceData.First(x => x.ParentDataItemID == thisChainItemID);
                chain.LogicChainData.Add(item);
                logicChainSourceData.Remove(item);
                thisChainItemID = item.ID;
            }
            return chain;
        }
    }

    /// <summary>
    /// A tag on the logic chain.
    /// </summary>
    public class LogicChainItem
    {
        /// <summary>
        /// Unique ID of this item.
        /// </summary>
        public long ID { get; set; }
        /// <summary>
        /// Which tag this item describes.
        /// </summary>
        public long TagID { get; set; }
        /// <summary>
        /// Which logic chain this item belongs to.
        /// </summary>
        public long ChainID { get; set; }
        /// <summary>
        /// If this chain belongs to a PropertyItem, this ID is the ID of the PropertyItem. Or its value can be ignored.
        /// </summary>
        public int RestrictionPropertyID { get; set; }
        /// <summary>
        /// The parent tag which this item connected. If there's no parent, it's value was -1.
        /// </summary>
        public long ParentDataItemID { get; set; }
        /// <summary>
        /// The property item which this item used to connect its parent tag. If there's no parent, it's value was -1.
        /// </summary>
        public long ParentPropertyItemID { get; set; }
        public string ParentPropertyItemName { get; set; } = string.Empty;
        /// <summary>
        /// The tag used as an item on the logic chain, -1 when as any tag.
        /// </summary>
        public long OnChainTagID { get; set; }
        public string OnChainTagName { get; set; } = string.Empty;
    }

    public class LogicChainConnectInfo
    {
        public long ParentTagID { get; set; }
        public long PropertyID { get; set; }
    }
}
