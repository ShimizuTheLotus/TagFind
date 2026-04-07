using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagFind.Classes.Extensions;

namespace TagFind.Classes.XAMLDataConverters
{
    public class Int32ToValueWarningStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Int32 val = (Int32)value;
            string raw = LocalizedString.GetLocalizedString("Code/CS/ValueWarnings");
            raw = raw.FormatLocalizedStringWithParameters(new() { { "value", val } });
            return raw;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
