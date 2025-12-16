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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DBWildItemsManagePage : Page, IDBContentAccessiblePage
    {
        public DBContentManager ContentManager { get; set; } = new();
        public DBWildItemsManagePage()
        {
            InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is DBContentManager dBContentManager)
            {
                if (dBContentManager != null)
                {
                    ContentManager = dBContentManager;
                    if (!ContentManager.Connected)
                    {
                        ContentManager.OpenDB();
                    }
                }
            }

            UpdateWildItemList();
        }


        private async void UpdateWildItemList()
        {
            if (ContentManager.Connected)
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    DataItemListView.DataItemCollection.Clear();
                    DataItemIsEmptyTextBlock.Visibility = Visibility.Collapsed;
                });

                var batch = new List<DataItem>();
                int batchSize = 20;// Use batch to avoid animation being interrupted.   
                await foreach (DataItem dataItem in ContentManager.DataItemGetWildItems())
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
        }
    }
}
