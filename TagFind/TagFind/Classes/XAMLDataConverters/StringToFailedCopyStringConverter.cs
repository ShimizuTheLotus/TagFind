using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagFind.Classes.Extensions;

namespace TagFind.Classes.XAMLDataConverters
{
    public class StringToFailedCopyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string s = LocalizedString.GetLocalizedString("Code/CS/FailedCopyString");
            
            if (value is string str)
            {
                s = s.FormatLocalizedStringWithParameters(new Dictionary<string, object>() { { "value", str } });
                return s;
            }
            s = s.FormatLocalizedStringWithParameters(new Dictionary<string, object>() { { "value", "[UNKNOWN]" } });
            return s;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
