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
using TagFind.Classes.Extensions;
using TagFind.Interfaces;
using TagFind.Interfaces.IPageNavigationParameter;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static TagFind.Classes.DataTypes.PageNavigateParameter;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DBContentDetailPage : Page, IDatabaseSearchConditionEditablePage
    {
        DataItem _dataItem { get; set; } = new();
        DBContentManager? _dbContentManager { get; set; }
        public ObservableCollection<ExplorerFolder> Path { get; set; } = [];

        public ObservableCollection<SearchCondition> searchConditions { get; set; } = [];

        public DBContentDetailPage()
        {
            InitializeComponent();
            OpenFileButton.Click += OpenFileButton_Click;
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            _dataItem.RefPath.OpenReferencedFileByDefaultProgram();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is DataItem dataItem)
            {
                _dataItem = dataItem;
                ApplyDataItem(_dataItem);
            }

            if (e.Parameter is IDataItemParameter itemParameter)
            {
                _dataItem = itemParameter.DataItem;
                ApplyDataItem(_dataItem);
            }
            if (e.Parameter is IDBContentManagerParameter dbParameter)
            {
                _dbContentManager = dbParameter.DBContentManager;
            }
            if (e.Parameter is ISearchConditionsParameter conditionParameter)
            {
                searchConditions = conditionParameter.SearchConditions.Duplicate();
            }
            if (e.Parameter is IExplorerPathParameter expParameter)
            {
                Path = expParameter.Path;
            }
        }

        public async void ApplyDataItem(DataItem dataItem)
        {
            string path = dataItem.RefPath;
            ItemImage.Height = 200;
            if (!path.IsImageFile() && !path.IsVideoFile())
            {
                ItemImage.Source = await path.GetThumbnail(200, 200);
            }
            else
            {
                ItemImage.Source = await path.GetImageThumbnail(-1, 200);
            }

            if (path != string.Empty)
            {
                OpenFileButton.Visibility = Visibility.Visible;
                if (path.IsValidFilePath())
                {
                    OpenFileButton.IsEnabled = true;
                    if (!path.HasFileAccess(FileExtensions.AccessType.Read))
                    {
                        OpenFileButton.IsEnabled = false;
                    }
                }
                else
                {
                    OpenFileButton.IsEnabled = false;
                }
            }
            else
            {
                OpenFileButton.Visibility= Visibility.Collapsed;
            }
            TitleTextBlock.Text = dataItem.Title;
            DescriptionTextBlock.Text = dataItem.Description;
            TagsWrapPanel.ItemsSource = dataItem.ItemTags;
        }

        public void AddConditionAndNavigateToExplorerPage(SearchCondition condition)
        {
            searchConditions.Add(condition);
            DataItemExplorerPageNavigationParameter parameters = new();
            parameters.DBContentManager = _dbContentManager;
            parameters.SearchConditions = searchConditions.Duplicate();
            parameters.Path = this.Path;
            Frame.Navigate(typeof(DBContentExplorerPage), parameters);
        }
    }
}
