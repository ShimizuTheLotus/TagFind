using Microsoft.UI;
using Microsoft.UI.Dispatching;
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
using TagFind.Classes.DB;
using TagFind.Classes.Extensions;
using TagFind.Interfaces;
using TagFind.UI;
using TagFind.UI.ContentDialogPages;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using static TagFind.UI.ContentDialogPages.CreateDatabaseContentDialogPage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class DBListPage : Page
{
    private DBListManager _listManager = new();
    private DBContentManager _contentManager = new();

    private List<MetaData> DatabaseInfoList = [];
    public DBListPage()
    {
        InitializeComponent();
        UpdateDatabaseList();
        DatabaseListView.RequestOpenDatabase += DatabaseListView_RequestOpenDatabase;
        
    }

    private void DatabaseListView_RequestOpenDatabase(object sender, long ID)
    {
        string path = DatabaseInfoList.First(x => x.ID == ID).Path;
        if (File.Exists(path))
        {
            Frame.Navigate(typeof(DBViewPage), path);
        }
    }

    private async void CreateDatabaseDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (sender.Content is CreateDatabaseContentDialogPage content)
        {
            string filePath = content.SelectedDatabasePath;
            if (!File.Exists(filePath))
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(System.IO.Path.GetDirectoryName(filePath));
                await folder.CreateFileAsync(System.IO.Path.GetFileName(filePath));
                if (File.Exists(filePath))
                {
                    _contentManager.OpenDB(filePath);
                    if (_contentManager.Connected)
                    {
                        _contentManager.InitializeDB();
                        await _contentManager.EditMeta("TDBVersion", "0.0.0");
                        await _contentManager.EditMeta("Description", content.Description);
                        _listManager.Add(filePath, content.Description);
                        _contentManager.CloseDB();
                        UpdateDatabaseList();
                    }
                }
            }
        }

    }

    private void UpdateDatabaseList()
    {
        DatabaseInfoList = [];
        _listManager.GetList(ref DatabaseInfoList);
        DatabaseListView.DatabaseList = DatabaseInfoList;
    }

    private async void AddDatabaseButton_Click(object sender, RoutedEventArgs e)
    {
        ContentDialog CreateDatabaseDialog = new();
        CreateDatabaseDialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        CreateDatabaseDialog.TitleTemplate = CreateTitleTemplate();
        CreateDatabaseDialog.XamlRoot = this.XamlRoot;
        CreateDatabaseDialog.PrimaryButtonText = GetLocalizedString("CreateDatabaseDialog/PrimaryButtonText");
        CreateDatabaseDialog.SecondaryButtonText = GetLocalizedString("CreateDatabaseDialog/SecondaryButtonText");
        CreateDatabaseDialog.DefaultButton = ContentDialogButton.Primary;
        CreateDatabaseDialog.IsPrimaryButtonEnabled = false;

        CreateDatabaseContentDialogPage content = new();
        RequestChangeDialogPrimaryButtonStatusEventHandler handler = (newValue) =>
        {
            CreateDatabaseDialog.IsPrimaryButtonEnabled = newValue;
        };
        content.RequestChangeDialogPrimaryButtonStatus += handler;
        CreateDatabaseDialog.Content = content;
        CreateDatabaseDialog.PrimaryButtonClick += CreateDatabaseDialog_PrimaryButtonClick;

        await CreateDatabaseDialog.ShowAsync();
        CreateDatabaseDialog.PrimaryButtonClick -= CreateDatabaseDialog_PrimaryButtonClick;
        content.RequestChangeDialogPrimaryButtonStatus -= handler;
    }

    private DataTemplate CreateTitleTemplate()
    {
        string xaml = @"
        <DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                      xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
            <StackPanel Orientation='Horizontal'>
                <Image Source='ms-appx:///Assets/SmallLogo.png' Width='40' Height='40' Margin='10,0'/>
                <TextBlock x:Uid='CreateDatabaseTextBlock'/>
            </StackPanel>
        </DataTemplate>";

        return (DataTemplate)Microsoft.UI.Xaml.Markup.XamlReader.Load(xaml);
    }

    public string GetLocalizedString(string key)
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

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(SettingsPage));
    }

    private void RemoveDatabaseReferenceButton_Click(object sender, RoutedEventArgs e)
    {

    }
}
