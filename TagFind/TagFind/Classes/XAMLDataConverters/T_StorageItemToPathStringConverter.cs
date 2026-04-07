using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagFind.Classes.DataTypes;
using TagFind.Classes.Extensions;
using Windows.Storage;

namespace TagFind.Classes.XAMLDataConverters
{
    public class T_StorageItemToPathStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            T_StorageItem item = (T_StorageItem)value;
            if (item.Item == null)
            {
                return LocalizedString.GetLocalizedString("Code/CS/NoPath");
            }
            if (item.IsStorageFile == true)
            {
                StorageFile storageFile = (StorageFile)item.Item;
                return storageFile.Path;
            }
            else if (item.IsStorageFile == false)
            {
                StorageFolder storageFolder = (StorageFolder)item.Item;
                return storageFolder.Path;
            }
            return LocalizedString.GetLocalizedString("Code/CS/NoPath");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
