using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes.XAMLDataConverters
{
    public class BooleanToVisibilityReversedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool boolValue = (bool)value;
            Visibility visibility = boolValue ? Visibility.Collapsed : Visibility.Visible;
            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
