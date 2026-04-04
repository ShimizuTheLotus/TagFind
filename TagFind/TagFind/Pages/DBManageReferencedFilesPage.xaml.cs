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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using TagFind.Classes.DataTypes;
using TagFind.Classes.DB;
using TagFind.Classes.Extensions;
using TagFind.Interfaces;
using TagFind.Interfaces.IPageNavigationParameter;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ManageReferencedFilesPage : Page, IDBContentAccessiblePage, INotifyPropertyChanged
    {
        public DBContentManager ContentManager { get; set; } = new();
        public bool IsFirstTimeLoaded = true;

        public CancellationTokenSource GetReferencedCancellationTokenSource { get; set; } = new();
        public long FileReferenceCounter { get => ReferencedFileInfos.Count; }
        public long ValidFileReferenceCounter { get; set; } = 0;
        public List<ReferencedFileInfo> ReferencedFileInfos { get; set; } = [];
        public bool IsGettingReferencedFileInfos
        {
            get => _isGettingReferencedFileInfos;
            set
            {
                if (_isGettingReferencedFileInfos != value)
                {
                    _isGettingReferencedFileInfos = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsGettingReferencedFileInfos)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsReadyForPack)));
                }
            }
        }
        private bool _isGettingReferencedFileInfos = false;

        public bool IsReadyForPack => !IsGettingReferencedFileInfos && PackStorageFolder != null;


        public StorageFolder? PackStorageFolder
        {
            get => _packStorageFolder;
            set
            {
                if (_packStorageFolder != value)
                {
                    _packStorageFolder = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PackStorageFolder)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsReadyForPack)));
                }
            }
        }
        private StorageFolder? _packStorageFolder = null;

        public ManageReferencedFilesPage()
        {
            InitializeComponent();
            this.Loaded += ManageReferencedFilesPage_Loaded;

            this.DataContext = this;
        }

        private async void ManageReferencedFilesPage_Loaded(object sender, RoutedEventArgs e)
        {
            await GetReferencedFileInfos();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is DBContentManager dBContentManager)
            {
                ContentManager = dBContentManager;
            }
            else if (e.Parameter is IDBContentManagerParameter dBContentManagerParameter)
            {
                if (dBContentManagerParameter.DBContentManager != null)
                {
                    ContentManager = dBContentManagerParameter.DBContentManager;
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            GetReferencedCancellationTokenSource.Cancel();
        }

        public async Task GetReferencedFileInfos()
        {
            GetReferencedCancellationTokenSource = new();

            ValidFileReferenceCounter = 0;
            ReferencedFileInfos.Clear();
            IsGettingReferencedFileInfos = true;

            try
            {
                await foreach (ReferencedFileInfo info in ContentManager.DataItemFastSearchGetAllReferencedFileInfos(GetReferencedCancellationTokenSource.Token))
                {
                    ReferencedFileInfos.Add(info);
                    bool exists = await Task.Run(() => File.Exists(info.Path));
                    if (exists)
                    {
                        ValidFileReferenceCounter++;
                        FileInfo fileInfo = new FileInfo(info.Path);
                        info.StorageSize = await Task.Run(() => new FileInfo(info.Path).Length);
                        string rawString = LocalizedString.GetLocalizedString("FileReferenceCountValueTextBlock/Text");
                        string processedString = rawString.FormatLocalizedStringWithParameters(new Dictionary<string, object>()
                        {
                            { "valid_count", ValidFileReferenceCounter },
                            { "ref_count", FileReferenceCounter }
                        });
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            try
                            {
                                FileReferenceCountValueTextBlock.Text = processedString;
                            }
                            catch { }
                        });
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Operation was canceled, do nothing
            }

            IsGettingReferencedFileInfos = false;
        }

        private async void SelectPackPathPathButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new Microsoft.UI.Xaml.Window();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            Microsoft.Windows.Storage.Pickers.FolderPicker folderPicker = new(windowId);
            var result = await folderPicker.PickSingleFolderAsync();
            window.Close();
            if (result != null)
            {
                string path = result.Path;
                PackStorageFolder = await StorageFolder.GetFolderFromPathAsync(path);
            }
        }

        private async void PackButton_Click(object sender, RoutedEventArgs e)
        {
            List<ReferencedFilePackStatusInfo> referencedFilePackStatusInfos = [];
            if (PackStorageFolder is null)
            {
                // Selected folder does not exist.
                DispatcherQueue.TryEnqueue(() =>
                {
                    PackStatusTextBlock.Text = LocalizedString.GetLocalizedString("Code/CS/Error_SelectedFolderNotExistsOrNoPermission");
                });
                return;
            }
            StorageFolder contentFolder = await PackStorageFolder.CreateFolderAsync("content", CreationCollisionOption.OpenIfExists);

            foreach (ReferencedFileInfo info in ReferencedFileInfos)
            {
                ReferencedFilePackStatusInfo referencedFilePackStatusInfo = new()
                {
                    ReferencedFileInfo = info
                };
                referencedFilePackStatusInfos.Add(referencedFilePackStatusInfo);
                if (File.Exists(info.Path))
                {
                    try
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            string s = LocalizedString.GetLocalizedString("Code/CS/Copying_Value");
                            PackStatusTextBlock.Text = s.FormatLocalizedStringWithParameters(new Dictionary<string, object>() { { "value", info.Path } });
                        });
                        StorageFile storageFile = await StorageFile.GetFileFromPathAsync(info.Path);
                        await storageFile.StorageWithPath(contentFolder);
                        referencedFilePackStatusInfo.FileNavigationStatus = FileNavigationStatus.Succeeded;
                    }
                    catch (Exception ex)
                    {
                        referencedFilePackStatusInfo.FileNavigationStatus = FileNavigationStatus.Failed;
                        referencedFilePackStatusInfo.Exception = ex.Message;
                    }
                }
                else
                {
                    referencedFilePackStatusInfo.FileNavigationStatus = FileNavigationStatus.Failed;
                    referencedFilePackStatusInfo.Exception = LocalizedString.GetLocalizedString("Code/CS/FileNotExistOrNoPermissionToAccess");
                }
            }
            DispatcherQueue.TryEnqueue(() =>
            {
                PackStatusTextBlock.Text = LocalizedString.GetLocalizedString("Code/CS/CopyFinished");
            });

            ShowFailedPackList(referencedFilePackStatusInfos);
        }

        public async void ShowFailedPackList(List<ReferencedFilePackStatusInfo> sourceList)
        {
            List<ReferencedFilePackStatusInfo> failedFilePackStatusInfos = sourceList.Where(x => x.FileNavigationStatus == FileNavigationStatus.Failed).ToList();
            PackFailedInfoListView.ItemsSource = failedFilePackStatusInfos;
        }
    }
}
