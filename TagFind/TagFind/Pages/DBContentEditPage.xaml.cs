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
using System.Threading.Tasks;
using TagFind.Classes.DataTypes;
using TagFind.Classes.DB;
using TagFind.Interfaces;
using TagFind.Interfaces.IPageNavigationParameter;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static TagFind.Classes.Consts.DB.UserDB;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DBContentEditPage : Page, IDBContentAccessiblePage, IAllowAddNewTagUsingDataItemTagEditorPage
    {
        public DBContentManager ContentManager { get; set; } = new();

        DataItem _dataItem = new();

        public DBContentEditPage()
        {
            InitializeComponent();

            this.Loaded += DBContentEditPage_Loaded;
        }

        private void DBContentEditPage_Loaded(object sender, RoutedEventArgs e)
        {
            DataItemEditor.RequestSaveContent += DataItemEditor_RequestSaveContent;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            DataItemEditor.RequestSaveContent -= DataItemEditor_RequestSaveContent;
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
                ContentManager = dBContentManagerParameter.DBContentManager ?? new();
            }
            if (e.Parameter is IDataItemParameter dataItemParameter && e.Parameter is IExplorerPathParameter explorerPathParameter)
            {
                long parentID = explorerPathParameter.Path.Count > 0 ? explorerPathParameter.Path[^1].ID : 0;
                dataItemParameter.DataItem.ParentID = parentID;
                _dataItem = dataItemParameter.DataItem;
                DataItemEditor.EditedDataItem = _dataItem;
            }
        }

        private async void DataItemEditor_RequestSaveContent(object sender, Classes.DataTypes.DataItem dataItem)
        {
            if (dataItem.ID != -1)
            {
                ContentManager?.DataItemUpdate(dataItem);
            }
            else
            {
                ContentManager?.DataItemAdd(dataItem);
            }

            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}
