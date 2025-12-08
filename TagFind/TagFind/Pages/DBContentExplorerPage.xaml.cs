using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
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
using TagFind.UI.ContentDialogPages;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT;
using static TagFind.Classes.Consts.DB.UserDB;
using static TagFind.Classes.DataTypes.PageNavigateParameter;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DBContentExplorerPage : Page, IDBContentAccessiblePage, IDatabaseSearchConditionEditablePage
    {
        public DBContentManager ContentManager { get; set; } = new();

        public ObservableCollection<SearchCondition> searchConditions { get; set; } = new();

        // Do not change the ID to -1, edit page won't allow edit when the path starts with -1.
        public ObservableCollection<ExplorerFolder> Path { get; set; } = [new() { Name = "Root", ID = 0 }];

        public DBContentExplorerPage()
        {
            InitializeComponent();
            this.Loaded += DBContentExplorerPage_Loaded;
            DataItemListView.RequestOpenDataItemDetail += DataItemListView_RequestOpenDataItemDetail;
            DataItemListView.RequestOpenDataItemAsFolder += DataItemListView_RequestOpenDataItemAsFolder;
            ConditionTokenizedSuggestBox.RequestSearch += ConditionTokenizedSuggestBox_RequestSearch;
            BreadcrumbBar.ItemsSource = Path;
            BreadcrumbBar.ItemClicked += BreadcrumbBar_ItemClicked;
            SearchModeSwitcher.SearchModeChanged += SearchModeSwitcher_SearchModeChanged;

            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void SearchModeSwitcher_SearchModeChanged(object sender, SearchModeEnum searchMode)
        {
            ConditionTokenizedSuggestBox_RequestSearch(this, ConditionTokenizedSuggestBox.SearchConditions);
        }

        private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
            var items = sender.ItemsSource as ObservableCollection<ExplorerFolder>;
            if (items == null) return;
            for (int i = items.Count - 1; i >= args.Index + 1; i--)
            {
                items.RemoveAt(i);
            }
            Path = items;
            ConditionTokenizedSuggestBox_RequestSearch(this, ConditionTokenizedSuggestBox.SearchConditions);
        }

        private async void DataItemListView_RequestOpenDataItemAsFolder(object sender, DataItem dataItem)
        {
            if (SearchModeSwitcher.SearchMode == SearchModeEnum.Layer)
            {
                Path.Add(new() { Name = dataItem.Title, ID = dataItem.ID });
            }
            else
            {
                SearchModeSwitcher.SearchMode = SearchModeEnum.Layer;
                if (ContentManager.Connected)
                {
                    Path = await ContentManager.GetDataItemPath(dataItem.ID);
                }
            }
            BreadcrumbBar.ItemsSource = Path;
            ConditionTokenizedSuggestBox_RequestSearch(this, ConditionTokenizedSuggestBox.SearchConditions);
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
            //List<DataItem> results = await ContentManager.DataItemsSearchViaSearchConditionsAsync(searchConditions, config);

            //DispatcherQueue.TryEnqueue(() =>
            //{
            //    DataItemListView.DataItemCollection = new ObservableCollection<DataItem>(results);
            //    DataItemIsEmptyTextBlock.Visibility = results.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            //});
            SearchAndSortModeInfo searchAndSortModeInfo = new()
            {
                SortDirection = SortDirectionEnum.ASC,
                SortMode = SortModeEnum.ID,
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

        private void DBContentExplorerPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateCommandBarState(DataItemListView.AllowSelecting);
            ConditionTokenizedSuggestBox.ApplyConditions(searchConditions); 
        }

        private async void DataItemListView_RequestOpenDataItemDetail(object sender, Classes.DataTypes.DataItem dataItem)
        {
            ObservableCollection<ExplorerFolder> path = [];
            if (SearchModeSwitcher.SearchMode != SearchModeEnum.Layer)
            {
                if (ContentManager.Connected)
                {
                    path = await ContentManager.GetDataItemPath(dataItem.ID);
                }
            }
            else
            {
                path = this.Path;
            }
            DataItemDetailPageNavigationParameter parameters = new()
            {
                DBContentManager = ContentManager,
                SearchConditions = searchConditions,
                DataItem = dataItem,
                Path = path
            };
            Frame.Navigate(typeof(DBContentDetailPage), parameters);
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
                UpdateContentList();
            }
            if (e.Parameter is IDBContentManagerParameter dBContentManagerParameter)
            {
                ContentManager = dBContentManagerParameter.DBContentManager ?? new();
                if (ContentManager != null)
                {
                    if (!ContentManager.Connected)
                    {
                        ContentManager.OpenDB();
                    }
                    UpdateContentList();
                }
            }
            if (e.Parameter is ISearchConditionsParameter searchConditionsParameter)
            {
                searchConditions = searchConditionsParameter.SearchConditions;
                ConditionTokenizedSuggestBox.ApplyConditions(searchConditions);
            }
            if (e.Parameter is IExplorerPathParameter explorerPathParameter)
            {
                Path = explorerPathParameter.Path;
            }
        }

        private async void UpdateContentList()
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
        }

        private void UpdateCommandBarState(bool selectionState)
        {
            if (selectionState)
            {
                AddAppBarButton.Visibility = Visibility.Collapsed;
                // Temporarily hide because now I'll upload the app to Microsoft Store and this button have no function now.
                //MoveAppBarButton.Visibility = Visibility.Visible;
                SelectAppBarButtonFontIcon.Glyph = "\xE73D";
                DeleteAppBarButton.Visibility = Visibility.Visible;
            }
            else
            {
                AddAppBarButton.Visibility = Visibility.Visible;
                MoveAppBarButton.Visibility = Visibility.Collapsed;
                SelectAppBarButtonFontIcon.Glyph = "\xE73A";
                DeleteAppBarButton.Visibility = Visibility.Collapsed;
            }
        }

        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            DataItemEditPageNavigationParameter parameters = new()
            {
                DBContentManager = this.ContentManager,
                DataItem = new() { ID = -1 },
                Path = this.Path
            };

            Frame.Navigate(typeof(DBContentEditPage), parameters);
        }

        private void MoveAppBarButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SortAppBarButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SelectAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            bool val = DataItemListView.AllowSelecting;
            DataItemListView.AllowSelecting = !val;
            UpdateCommandBarState(!val);
        }


        private async void DeleteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog DeleteSelectionDialog = new();
            DeleteSelectionDialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            DeleteSelectionDialog.Title = GetLocalizedString("ConfirmDeletingDataItemsDialogTitle/String");
            DeleteSelectionDialog.XamlRoot = this.XamlRoot;
            DeleteSelectionDialog.PrimaryButtonText = GetLocalizedString("Delete/String");
            DeleteSelectionDialog.SecondaryButtonText = GetLocalizedString("Cancel/String");
            DeleteSelectionDialog.DefaultButton = ContentDialogButton.Secondary;

            ConfirmDeleteDataItemContentDialogPage content = new();
            DeleteSelectionDialog.Content = content;
            DeleteSelectionDialog.PrimaryButtonClick += DeleteSelectionDialog_PrimaryButtonClick;
            await DeleteSelectionDialog.ShowAsync();
            DeleteSelectionDialog.PrimaryButtonClick -= DeleteSelectionDialog_PrimaryButtonClick;
        }

        private async void DeleteSelectionDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            bool removeChildItems = false;
            if (sender.Content is ConfirmDeleteDataItemContentDialogPage content)
            {
                removeChildItems = content.DeleteChildItems;
            }
            List<long> rootIDsToDelete = [];
            var rootIDs = DataItemListView.SelectedItems;
            if (rootIDs is IList<object> rootIDList)
            {
                foreach (var rootIDItem in rootIDList)
                {
                    if (rootIDItem is DataItem dataItem)
                    {
                        rootIDsToDelete.Add(dataItem.ID);
                    }
                }
            }

            if (ContentManager != null)
            {
                await ContentManager.DataItemBatchDelete(rootIDsToDelete, removeChildItems);
                UpdateContentList();
            }
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

        public void AddConditionAndNavigateToExplorerPage(SearchCondition condition)
        {
            searchConditions.Add(condition);
            ConditionTokenizedSuggestBox.ApplyConditions(searchConditions);
        }
    }
}
