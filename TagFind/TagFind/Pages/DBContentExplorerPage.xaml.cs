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
using TagFind.UI.ContentDialogPages;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT;
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
            DataItemSearchConfig config = new()
            {
                ParentIDLimit = Path.Count > 0 ? Path[^1].ID : 0,
                SearchTitle = true,
                SearchDescription = true
            };
            ConditionTokenizedSuggestBox_RequestSearch(this, ConditionTokenizedSuggestBox.SearchConditions, config);
        }

        private void DataItemListView_RequestOpenDataItemAsFolder(object sender, DataItem dataItem)
        {
            Path.Add(new() { Name = dataItem.Title, ID = dataItem.ID });
            BreadcrumbBar.ItemsSource = Path;
            DataItemSearchConfig config = new()
            {
                ParentIDLimit = Path.Count > 0 ? Path[^1].ID : 0,
                SearchTitle = true,
                SearchDescription = true
            };
            ConditionTokenizedSuggestBox_RequestSearch(this, ConditionTokenizedSuggestBox.SearchConditions, config);
        }

        private async void ConditionTokenizedSuggestBox_RequestSearch(object sender, ObservableCollection<SearchCondition> searchConditions, DataItemSearchConfig? config = null)
        {
            this.searchConditions = searchConditions;
            DataItemListView.DataItemCollection = new ObservableCollection<DataItem>();

            if (config == null)
            {
                config = new()
                {
                    ParentIDLimit = Path.Count > 0 ? Path[^1].ID : 0,
                    SearchTitle = true,
                    SearchDescription = true
                };
            }
            List<DataItem> results = await ContentManager.DataItemsSearchViaSearchConditionsAsync(searchConditions, config);

            DispatcherQueue.TryEnqueue(() =>
            {
                DataItemListView.DataItemCollection = new ObservableCollection<DataItem>(results);
                DataItemIsEmptyTextBlock.Visibility = results.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        private void DBContentExplorerPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateCommandBarState(DataItemListView.AllowSelecting);
            ConditionTokenizedSuggestBox.ApplyConditions(searchConditions); 
        }

        private void DataItemListView_RequestOpenDataItemDetail(object sender, Classes.DataTypes.DataItem dataItem)
        {
            DataItemDetailPageNavigationParameter parameters = new()
            {
                DBContentManager = ContentManager,
                SearchConditions = searchConditions,
                DataItem = dataItem,
                Path = this.Path
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
                var value = await ContentManager.DataItemsGetChildOfParentItemAsync(0) ?? [];
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
                MoveAppBarButton.Visibility = Visibility.Visible;
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
