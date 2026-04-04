using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace TagFind.Classes.Extensions
{
    public static class FileStorageExtensions
    {
        public static async Task<bool> StorageWithPath(this StorageFile file, StorageFolder newtFolder)
        {
            bool isSucceeded = false;
            try
            {
                string path = file.Path;
                string diskName = Path.GetPathRoot(path) ?? string.Empty;
                diskName = diskName.Replace(":", "");
                if (string.IsNullOrWhiteSpace(diskName))
                {
                    return isSucceeded;
                }
                string relativePath = Path.GetRelativePath(diskName, path);
                string targetPath = Path.Combine(newtFolder.Path, relativePath);
                string targetDirectory = Path.GetDirectoryName(targetPath) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(targetDirectory))
                {
                    return isSucceeded;
                }
                StorageFolder targetFolder = await StorageFolder.GetFolderFromPathAsync(newtFolder.Path);
                var folderStack = targetDirectory.Split(Path.DirectorySeparatorChar);

                foreach (var folder in folderStack)
                {
                    if (!string.IsNullOrEmpty(folder))
                    {
                        targetFolder = await targetFolder.CreateFolderAsync(folder.Replace(":", ""), CreationCollisionOption.OpenIfExists);
                    }
                }
                StorageFile targetFile = await file.CopyAsync(targetFolder, Path.GetFileName(targetPath), NameCollisionOption.ReplaceExisting);
                isSucceeded = true;
            }
            catch
            {
                return isSucceeded;
            }
            return isSucceeded;
        }
    }
}
