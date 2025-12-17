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
using System.Threading.Tasks;
using TagFind.Classes.DataTypes;
using TagFind.Classes.DB;
using TagFind.Interfaces;
using TagFind.Interfaces.IPageNavigationParameter;
using TagFind.UI;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DBContentMoveToPage : Page, IDBContentAccessiblePage
    {
        public DBContentManager ContentManager { get; set; } = new();
        public List<DataItem> CarriedDataItems { get; set; } = [];
        public ObservableCollection<SearchCondition> searchConditions { get; set; } = new();

        // Do not change the ID to -1, edit page won't allow edit when the path starts with -1.
        public ObservableCollection<ExplorerFolder> Path { get; set; } = [new() { Name = "Root", ID = 0 }];
        SortDirectionEnum SortDirection { get; set; } = SortDirectionEnum.DESC;
        SortModeEnum SortMode = SortModeEnum.ID;

        public DBContentMoveToPage()
        {
            InitializeComponent();

            BreadcrumbBar.ItemsSource = Path;
        }


        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is IDBContentManagerParameter dBContentManagerParameter)
            {
                if (dBContentManagerParameter.DBContentManager != null)
                {
                    ContentManager = dBContentManagerParameter.DBContentManager;
                }
            }
            if (e.Parameter is IDataItemListParameter dataItemListParameter)
            {
                CarriedDataItems = dataItemListParameter.DataItemList;
            }

            PasteAppBarButton.IsEnabled = CarriedDataItems.Count > 0;
            await UpdateContentList();
        }

        private async void DataItemListView_RequestOpenDataItemAsFolder(object sender, Classes.DataTypes.DataItem dataItem)
        {
            if (ContentManager != null)
            {
                if (ContentManager.Connected)
                {
                    Path = await ContentManager.GetDataItemPath(dataItem.ID);
                }
                BreadcrumbBar.ItemsSource = Path;
                ConditionTokenizedSuggestBox_RequestSearch(this, ConditionTokenizedSuggestBox.SearchConditions);
            }
        }

        private async void PasteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContentManager.Connected)
            {
                if (Path.Count > 0)
                {
                    long newParentID = Path[^1].ID;
                    // Do not try to move items into themselves
                    if (!CarriedDataItems.Any(x => x.ID == newParentID))
                    {
                        foreach (DataItem dataItem in CarriedDataItems)
                        {
                            await ContentManager.DataItemMoveTo(dataItem.ID, newParentID);
                        }
                    }
                }
            }
            CarriedDataItems = [];
            PasteAppBarButton.IsEnabled = false;

            await UpdateContentList();
        }

        private async void ConditionTokenizedSuggestBox_RequestSearch(object sender, ObservableCollection<SearchCondition> searchConditions)
        {
            this.searchConditions = searchConditions;
            DataItemListView.DataItemCollection = new ObservableCollection<DataItem>();

            DataItemSearchConfig config = new()
            {
                SearchMode = SearchModeSwitcher.SearchMode,
                ParentOrAncestorIDLimit = Path.Count > 0 ? Path[^1].ID : 0,
                SearchTitle = true,
                SearchDescription = true
            };

            SearchAndSortModeInfo searchAndSortModeInfo = new()
            {
                SortDirection = SortDirection,
                SortMode = this.SortMode,
                TextMatchMode = TextMatchModeEnum.AllResults
            };

            DispatcherQueue.TryEnqueue(async () =>
            {
                DataItemListView.DataItemCollection.Clear();
                DataItemIsEmptyTextBlock.Visibility = Visibility.Collapsed;
            });
            var batch = new List<DataItem>();
            int batchSize = 20;// Use batch to avoid animation being interrupted.

            await foreach (DataItem dataItem in ContentManager.DataItemSearchViaSearchConditionsIterativeAsync(searchConditions, config, searchAndSortModeInfo))
            {
                batch.Add(dataItem);
                if (batch.Count >= batchSize)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        foreach (var item in batch)
                        {
                            DataItemListView.DataItemCollection.Add(item);
                        }
                    });
                    batch.Clear();
                    await Task.Delay(1);
                }
            }
            if (batch.Count > 0)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    foreach (var item in batch)
                    {
                        DataItemListView.DataItemCollection.Add(item);
                    }
                });
            }
            DispatcherQueue.TryEnqueue(() =>
            {
                DataItemIsEmptyTextBlock.Visibility =
                    DataItemListView.DataItemCollection.Count == 0 ?
                    Visibility.Visible : Visibility.Collapsed;
            });
        }

        private async Task UpdateContentList()
        {
            if (ContentManager != null && ContentManager.Connected)
            {
                long parentID = this.Path.Count > 0 ? this.Path[^1].ID : 0;
                var value = await ContentManager.DataItemsGetChildOfParentItemAsync(parentID) ?? [];
                DispatcherQueue.TryEnqueue(() =>
                {
                    DataItemListView.DataItemCollection = value;
                });
            }
            DispatcherQueue.TryEnqueue(() =>
            {
                DataItemIsEmptyTextBlock.Visibility =
                    DataItemListView.DataItemCollection.Count == 0 ?
                    Visibility.Visible : Visibility.Collapsed;
            });
        }
    }
}
