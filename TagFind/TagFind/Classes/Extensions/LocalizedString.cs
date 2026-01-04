using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes.Extensions
{
    public static class LocalizedString
    {
        public static string GetLocalizedString(string key)
        {
            try
            {
                var resourceLoader = new Microsoft.Windows.ApplicationModel.Resources.ResourceLoader();
                return resourceLoader.GetString(key);
            }
            catch
            {
                return "{Resource Load Failed}";
            }
        }

        public static void FormatLocalizedStringWithParameters(this string originalString, Dictionary<string, object> parameters)
        {
            foreach (var parameter in parameters)
            {
                originalString = originalString.Replace($"{{{parameter.Key}}}", parameter.Value.ToString());
            }
        }
    }
}
