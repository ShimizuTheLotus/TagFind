using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagFind.Classes.Extensions;
using Windows.Storage;

namespace TagFind.Classes.XAMLDataConverters
{
    public class StorageFolderToPathStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            StorageFolder? storageFolder = (StorageFolder?)value;
            if(storageFolder != null)
                return storageFolder.Path;
            return LocalizedString.GetLocalizedString("Code/CS/NotSelected");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
