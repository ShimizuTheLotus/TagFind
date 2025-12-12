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
using TagFind.Interfaces.IPageNavigationParameter;
using TagFind.UI;
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
    public sealed partial class DBTagPoolPage : Page
    {
        public DBContentManager ContentManager { get; set; } = new();

        public DBTagPoolPage()
        {
            InitializeComponent();
            TagSearchAutoSuggestBox.TextChanged += TagSearchAutoSuggestBox_TextChanged;
            TagSearchAutoSuggestBox.QuerySubmitted += TagSearchAutoSuggestBox_QuerySubmitted;
            this.Loaded += DBTagPoolPage_Loaded;
        }

        private async void DBTagPoolPage_Loaded(object sender, RoutedEventArgs e)
        {
            await GetTagList();
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            await GetTagList();
        }

        private void TagSearchAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (TagListListView.SelectedIndex < 0) return;
            if (TagListListView.SelectedItem is Tag selectedTag)
            {
                TagEditPageNavigationParameter parameters = new()
                {
                    DBContentManager = this.ContentManager,
                    Tag = selectedTag
                };
                Frame.Navigate(typeof(DBTagEditPage), parameters);
            }
        }

        private async void TagSearchAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            await GetTagList();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (ContentManager.Connected)
            {
                await GetTagList();
            }
            if (e.Parameter is DBContentManager manager)
            {
                ContentManager = manager;
                if (!manager.Connected)
                {
                    manager.OpenDB();
                }
                await GetTagList();
            }
            if (e.Parameter is IDBContentManagerParameter dBContentManagerParameter)
            {
                if (dBContentManagerParameter.DBContentManager != null)
                {
                    ContentManager = dBContentManagerParameter.DBContentManager;
                    if (!ContentManager.Connected)
                    {
                        ContentManager.OpenDB();
                    }
                    await GetTagList();
                }
            }
        }

        public async Task GetTagList()
        {
            ObservableCollection<Tag> tagList = [];
            if (ContentManager != null)
            {
                string searchString = string.Empty;
                if (TagSearchAutoSuggestBox != null && TagSearchAutoSuggestBox.Text != string.Empty)
                {
                    searchString = TagSearchAutoSuggestBox.Text + "%";
                }
                tagList = await ContentManager.TagPoolGetTagList(searchString);
            }

            DispatcherQueue.TryEnqueue(() =>
            {
                if (TagListListView == null) return;
                try
                {
                    TagListListView.ItemsSource = tagList;
                }
                catch { }
            });
        }

        private void TagListListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TagListListView.SelectedIndex < 0) return;
            if (TagListListView.SelectedItem is Tag selectedTag)
            {
                TagEditPageNavigationParameter parameters = new()
                {
                    DBContentManager = this.ContentManager,
                    Tag = selectedTag
                };
                Frame.Navigate(typeof(DBTagEditPage), parameters);
            }
        }

        private void AddTagButton_Click(object sender, RoutedEventArgs e)
        {
            TagEditPageNavigationParameter parameters = new()
            {
                DBContentManager = this.ContentManager,
                Tag = new() { ID = -1 }
            };
            Frame.Navigate(typeof(DBTagEditPage), parameters);
        }

        private void DeleteTagButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
