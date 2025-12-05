using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ThirdPartyAttributionPage : Page
    {
        public ThirdPartyAttributionPage()
        {
            InitializeComponent();
            LoadText();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private async void LoadText()
        {
            ThirdPartyAttributionTextBlock.Text = await ReadTextFileFromAssets("Third-party/THIRD-PARTY-NOTICES.txt");
        }

        private async Task<string> ReadTextFileFromAssets(string fileName)
        {
            try
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(
                    new Uri($"ms-appx:///Assets/{fileName}"));
                return await FileIO.ReadTextAsync(file);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load file: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
