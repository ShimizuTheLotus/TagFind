using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes.Extensions
{
    public static class UIHelperText
    {
        public static void ShowHelperText(this FrameworkElement FrameworkElement, string LocalizedStringKey, Dictionary<string, object>? parameters = null)
        {
            Flyout flyout = new();
            flyout.OverlayInputPassThroughElement = FrameworkElement;
            TextBlock textBlock = new();
            string s = LocalizedString.GetLocalizedString(LocalizedStringKey);
            if (parameters != null)
            {
                s = s.FormatLocalizedStringWithParameters(parameters);
            }
            textBlock.Text = s;
            textBlock.MaxWidth = 320;
            textBlock.TextWrapping = TextWrapping.WrapWholeWords;
            flyout.Content = textBlock;
            flyout.ShowAt(FrameworkElement);
        }
    }
}
