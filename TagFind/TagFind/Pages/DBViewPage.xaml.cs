using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TagFind.Classes.DB;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DBViewPage : Page
    {
        private DBContentManager _dbContentManager = new();

        public DBViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string path)
            {
                _dbContentManager.OpenDB(path);
            }
            //ContentFrame.Navigate(typeof(DBContentExplorerPage), _dbContentManager);
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                if (args.SelectedItem is NavigationViewItem selectedItem)
                {
                    string selectedItemTag = selectedItem?.Tag?.ToString() ?? string.Empty;
                    if (selectedItemTag == "Exit")
                    {
                        Frame.GoBack();
                        return;
                    }
                    string pageName = "TagFind.Pages." + selectedItemTag;
                    Type? pageType = Type.GetType(pageName);
                    if (pageType != null)
                    {
                        ContentFrame.Navigate(pageType, _dbContentManager);
                    }
                }
            }
        }

        private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
            else if (Frame.CanGoBack)
            {
                _dbContentManager.CloseDB();
                Frame.GoBack();
            }
        }
    }
}
