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
using TagFind.Classes;
using TagFind.Classes.DB;
using TagFind.Classes.Extensions;
using TagFind.Interfaces;
using TagFind.Interfaces.IPageNavigationParameter;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DBDetailedInformationPage : Page, IDBContentAccessiblePage
    {
        public DBContentManager ContentManager { get; set; } = new();

        protected override void OnNavigatedTo(NavigationEventArgs e)
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

            // Load info
            DBNameTextBlock.Text = Path.GetFileNameWithoutExtension(ContentManager.DBPath);
            long fileLength = new FileInfo(ContentManager.DBPath).Length;
            StorageSizeValueTextBlock.Text = fileLength < 1024
                ? fileLength + " " + "bytes"
                : fileLength.ToSuitableStorageSize(SettingsValues.UseBinaryStorageUnits) + " (" + fileLength.ToString() + " " + "bytes" + ")";
        }

        public DBDetailedInformationPage()
        {
            InitializeComponent();
        }
    }
}
