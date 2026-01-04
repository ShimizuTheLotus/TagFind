using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TagFind.Classes.DataTypes;
using TagFind.Classes.DB;
using TagFind.Classes.Extensions;
using TagFind.Interfaces;
using TagFind.Interfaces.IPageNavigationParameter;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DBTagEditPage : Page, IDBContentAccessiblePage
    {
        public DBContentManager ContentManager { get; set; } = new();

        public DBTagEditPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is DBContentManager manager)
            {
                ContentManager = manager;
                if (!manager.Connected)
                {
                    manager.OpenDB();
                }
            }
            if (e.Parameter is IDBContentManagerParameter dBContentManagerParameter)
            {
                if (dBContentManagerParameter.DBContentManager != null)
                {
                    ContentManager = dBContentManagerParameter.DBContentManager;
                }
            }
            if (e.Parameter is ITagParameter tagParameter)
            {
                TagEditor.EditedTag = tagParameter.Tag;
            }

            DeleteButton.IsEnabled = TagEditor.EditedTag.ID != -1;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (TagEditor.EditedTag.ID == -1)
            {
                ContentManager?.TagPoolAddUniqueTag(TagEditor.EditedTag);
            }
            else
            {
                ContentManager?.TagPoolUpdateTag(TagEditor.EditedTag);
            }
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            TagEditor.EditedTag = new() { ID = -1 };
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog deleteTagDialog = new();
            deleteTagDialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            deleteTagDialog.Title = LocalizedString.GetLocalizedString("AreYouSureYouWantToRemoveThisTag/String");
            deleteTagDialog.Content = LocalizedString.GetLocalizedString("AllReferencesWillBeRemovedAlong/String");
            deleteTagDialog.XamlRoot = this.XamlRoot;
            deleteTagDialog.PrimaryButtonText = LocalizedString.GetLocalizedString("Remove/String");
            deleteTagDialog.SecondaryButtonText = LocalizedString.GetLocalizedString("Cancel/String");
            deleteTagDialog.DefaultButton = ContentDialogButton.Secondary;
            deleteTagDialog.PrimaryButtonClick += DeleteTagDialog_PrimaryButtonClick; ;
            await deleteTagDialog.ShowAsync();
            deleteTagDialog.PrimaryButtonClick -= DeleteTagDialog_PrimaryButtonClick;
        }

        private void DeleteTagDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ContentManager?.TagPoolRemoveTagByID(TagEditor.EditedTag.ID);
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}
