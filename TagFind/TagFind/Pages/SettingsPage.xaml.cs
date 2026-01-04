using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TagFind.Classes.Extensions;
using TagFind.UI.ContentDialogPages;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void ThirdPartyAttributionButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ThirdPartyAttributionPage));
        }

        private async void EnterTutorialButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog TutorialDialog = new();
            TutorialDialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            TutorialDialog.Title = LocalizedString.GetLocalizedString("TutorialTextBlock/Text");
            TutorialDialog.XamlRoot = this.XamlRoot;
            TutorialDialog.PrimaryButtonText = LocalizedString.GetLocalizedString("Close/String");
            TutorialDialog.DefaultButton = ContentDialogButton.Primary;
            TutorialDialog.Content = new TutorialContentDialogPage();

            await TutorialDialog.ShowAsync();
        }
    }
}
