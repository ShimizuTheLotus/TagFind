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

        public static string GetTargetStoragePath(string basePath, string originalPath)
        {
            if (string.IsNullOrWhiteSpace(basePath))
            {
                return originalPath;
            }
            string folderizedPath = originalPath.Replace(":", "");
            return Path.Combine(basePath, folderizedPath);
        }

        public static string GetFilePathWithDiskNameFromArchiveEntry(this string Path)
        {
            StringBuilder stringBuilder = new(Path);
            string contentEntryHead = Consts.FileArchive.ContentFolderName + "/";
            int index = Path.IndexOf(contentEntryHead);
            if (index != -1)
            {
                stringBuilder.Remove(0, contentEntryHead.Length);
                int slashIndex = stringBuilder.ToString().IndexOf("/");
                if (slashIndex != -1)
                {
                    stringBuilder.Insert(slashIndex, ":");
                }
                return stringBuilder.ToString();
            }
            return Path;
        }

        public static string GetRelativeFilePathWithContentFolder(this string s)
        {
            s = s.Replace(":", "");
            return System.IO.Path.Combine(Consts.FileArchive.ContentFolderName, s).Replace("\\", "/");
        }

        public static string DiskNameToFolder(this string Path)
        {
            return Path.Replace(":", "");
        }

        public static string PathToArchiveEntry(this string Path)
        {
            string s = Path.DiskNameToFolder();
            s = System.IO.Path.Combine(Consts.FileArchive.ContentFolderName, s);
            return s.Replace("\\", "/");
        }
    }
}
