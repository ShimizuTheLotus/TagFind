using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes.DataTypes
{
    public class DBItemIDReferenceFilePathInfo : Dictionary<string, HashSet<long>>
    {
        private Dictionary<string, long> FileSize = [];
        public void Add(long ItemID, string ReferenceFilePath)
        {
            if (!ContainsKey(ReferenceFilePath))
            {
                Add(ReferenceFilePath, []);
            }
            if (TryGetValue(ReferenceFilePath, out HashSet<long>? itemIDs))
            {
                itemIDs.Add(ItemID);
            }
        }
        private void Add(ReferencedFileInfo ReferencedFileInfo)
        {
            if (!ContainsKey(ReferencedFileInfo.Path))
            {
                Add(ReferencedFileInfo.Path, []);
            }
            if (TryGetValue(ReferencedFileInfo.Path, out HashSet<long>? itemIDs))
            {
                itemIDs.Add(ReferencedFileInfo.DataItemID);
            }

            FileSize.Add(ReferencedFileInfo.Path, ReferencedFileInfo.StorageSize);
        }

        public IEnumerable<long> GetReferencingItems(string ReferenceFilePath)
        {
            if (TryGetValue(ReferenceFilePath, out HashSet<long>? itemIDs))
            {
                return itemIDs;
            }
            return [];
        }

        public async Task<long> GetStorageSize(string Path)
        {
            if (FileSize.TryGetValue(Path, out long value))
            {
                return value;
            }
            else
            {
                if (File.Exists(Path))
                {
                    return await Task.Run(() => new FileInfo(Path).Length);
                }
                return -1;
            }
        }
    }
}
