using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes.Extensions
{
    public static class StorageSizeExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="binaryUnits">true for 1024 per unit, false for 1000 per unit.</param>
        /// <param name="storageBytes">Size using bytes unit.</param>
        /// <returns></returns>
        public static string ToSuitableStorageSize(this long storageBytes, bool binaryUnits)
        {
            if (binaryUnits)
            {
                if (storageBytes < Math.Pow(2, 10))
                    return storageBytes.ToString() + " " + "bytes";
                else if (storageBytes < Math.Pow(2, 20))
                    return ((double)storageBytes / Math.Pow(2, 10)).ToString() + " " + "KiB";
                else if (storageBytes < Math.Pow(2, 30))
                    return ((double)storageBytes / Math.Pow(2, 20)).ToString() + " " + "MiB";
                else if (storageBytes < Math.Pow(2, 40))
                    return ((double)storageBytes / Math.Pow(2, 30)).ToString() + " " + "GiB";
                else if (storageBytes < Math.Pow(2, 50))
                    return ((double)storageBytes / Math.Pow(2, 40)).ToString() + " " + "TiB";
                else if (storageBytes < Math.Pow(2, 60))
                    return ((double)storageBytes / Math.Pow(2, 50)).ToString() + " " + "PiB";
                else if (storageBytes < Math.Pow(2, 70))
                    return ((double)storageBytes / Math.Pow(2, 60)).ToString() + " " + "EiB";
                else
                    return (storageBytes / Math.Pow(2, 70)).ToString() + " " + "ZiB";
            }
            else
            {
                if (storageBytes < Math.Pow(1000, 1))
                    return storageBytes.ToString() + " " + "bytes";
                else if (storageBytes < Math.Pow(1000, 2))
                    return ((double)storageBytes / Math.Pow(1000, 1)).ToString() + " " + "KB";
                else if (storageBytes < Math.Pow(1000, 3))
                    return ((double)storageBytes / Math.Pow(1000, 2)).ToString() + " " + "MB";
                else if (storageBytes < Math.Pow(1000, 4))
                    return ((double)storageBytes / Math.Pow(1000, 3)).ToString() + " " + "GB";
                else if (storageBytes < Math.Pow(1000, 5))
                    return ((double)storageBytes / Math.Pow(1000, 4)).ToString() + " " + "TB";
                else if (storageBytes < Math.Pow(1000, 6))
                    return ((double)storageBytes / Math.Pow(1000, 5)).ToString() + " " + "PB";
                else if (storageBytes < Math.Pow(1000, 7))
                    return ((double)storageBytes / Math.Pow(1000, 6)).ToString() + " " + "EB";
                else
                    return ((double)storageBytes / Math.Pow(1000, 7)).ToString() + " " + "ZB";
            }
        }
    }
}
