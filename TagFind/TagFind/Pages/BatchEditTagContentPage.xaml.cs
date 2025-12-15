using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using TagFind.Interfaces;
using TagFind.Classes.DB;
using TagFind.Interfaces.IPageNavigationParameter;
using TagFind.Classes.DataTypes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BatchEditTagContentPage : Page, IDBContentAccessiblePage
{
    public DBContentManager ContentManager { get; set; } = new();
    public List<DataItem> DataItemsBeingEdited { get; set; } = [];

    public BatchEditTagContentPage()
    {
        InitializeComponent();
    }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is IDBContentManagerParameter dBContentManager)
        {
            ContentManager = dBContentManager.DBContentManager ?? new();
        }
        if (e.Parameter is IDataItemListParameter dataItemListParameter)
        {
            DataItemsBeingEdited = dataItemListParameter.DataItemList;
        }
    }

    private async void ApplyChangeButton_Click(object sender, RoutedEventArgs e)
    {
        List<DataItem> _dataItemsBeingEdited = DataItemsBeingEdited;
        if (ApplyToSubitemsCheckBox.IsChecked == true)
        {
            if (ContentManager.Connected)
            {
                foreach (DataItem dataItem in DataItemsBeingEdited)
                {
                    foreach (DataItem item in await ContentManager.GetAllChildDataItemsDFS(dataItem.ID))
                    {
                        _dataItemsBeingEdited.Add(item);
                    }
                }
            }
        }
        foreach (DataItem item in _dataItemsBeingEdited)
        {
            item.AddTagsToDataItem(TagsToBeAddedEditor.ItemTags);
            item.MarkToRemoveTagsFromDataItem(TagsToBeDeletedEditor.ItemTags);
            if (ContentManager.Connected)
            {
                ContentManager.DataItemUpdate(item);
            }
        }
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }
}
