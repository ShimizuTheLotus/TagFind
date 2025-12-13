using Microsoft.Extensions.DependencyInjection;
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
using TagFind.Interfaces;
using TagFind.Pages;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, IMessagePushable
    {
        // Global service
        private readonly IServiceProvider? _serviceProvider;

        // Service manager
        private MessageManager MessageManager = ((App?)Application.Current)?.ServiceProvider?.GetRequiredService<MessageManager>() ?? new();

        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            _serviceProvider = ((App?)Application.Current)?.ServiceProvider;
            MainFrame.Navigate(typeof(DBListPage));

            if (App.Current != null)
            {
                App.Current.UnhandledException += Current_UnhandledException;
            }
        }

        private async void Current_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            ContentDialog unhandledExceptionMessageDialog = new();
            unhandledExceptionMessageDialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            unhandledExceptionMessageDialog.Title = "Unhandled Exception:";
            unhandledExceptionMessageDialog.Content = e.Message;
            unhandledExceptionMessageDialog.XamlRoot = this.Content.XamlRoot;
            unhandledExceptionMessageDialog.PrimaryButtonText = "OK";
            unhandledExceptionMessageDialog.DefaultButton = ContentDialogButton.Primary;
            unhandledExceptionMessageDialog.IsPrimaryButtonEnabled = false;
            await unhandledExceptionMessageDialog.ShowAsync();
        }

        public void ClearMessage()
        {

        }

        public void DeleteMessage(int messageId)
        {

        }

        public void PushMessage(MessageType messageType, string content)
        {

        }
    }
}
