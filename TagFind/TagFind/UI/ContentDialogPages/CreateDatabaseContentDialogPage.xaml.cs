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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI.ContentDialogPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateDatabaseContentDialogPage : Page
    {
        string SelectedFolderForCreatingDatabasePath = string.Empty;
        public string SelectedDatabasePath => Path.Combine(SelectedFolderForCreatingDatabasePath, SetDatabaseNameTextBox.Text.Trim() + ".db");
        public string Description => DescriptionTextBox.Text;

        public delegate void RequestChangeDialogPrimaryButtonStatusEventHandler(bool newStatus);
        public event RequestChangeDialogPrimaryButtonStatusEventHandler? RequestChangeDialogPrimaryButtonStatus;

        public CreateDatabaseContentDialogPage()
        {
            InitializeComponent();
            SetDatabaseNameTextBox.TextChanged += SetDatabaseNameTextBox_TextChanged;
        }

        private async void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new Microsoft.UI.Xaml.Window();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            Microsoft.Windows.Storage.Pickers.FolderPicker folderPicker = new(windowId);
            var result = await folderPicker.PickSingleFolderAsync();
            if (result != null)
            {
                SelectedFolderForCreatingDatabasePath = result.Path;
            }
            else
            {
                SelectedFolderForCreatingDatabasePath = string.Empty;
            }
            CreateDatabasePathTextBlock.Text = SelectedFolderForCreatingDatabasePath;
            CheckCreateDatabasePath();
        }

        private void CheckCreateDatabasePath()
        {
            if (SelectedFolderForCreatingDatabasePath != string.Empty)
            {
                // Database already exists.
                if (File.Exists(SelectedDatabasePath))
                {
                    RequestChangeDialogPrimaryButtonStatus?.Invoke(false);
                }
                else
                {
                    if (SetDatabaseNameTextBox.Text.Trim() == string.Empty)
                    {
                        RequestChangeDialogPrimaryButtonStatus?.Invoke(false);
                    }
                    else
                    {
                        RequestChangeDialogPrimaryButtonStatus?.Invoke(true);
                    }
                }
            }
            else
            {
                RequestChangeDialogPrimaryButtonStatus?.Invoke(false);
            }
        }
        private void SetDatabaseNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckCreateDatabasePath();
        }
    }
}
