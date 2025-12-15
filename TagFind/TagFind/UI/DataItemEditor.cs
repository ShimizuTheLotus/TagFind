using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TagFind.Classes.DataTypes;
using Windows.Storage;
using Windows.System;
using static TagFind.Classes.Consts.DB.UserDB;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI
{
    public sealed partial class DataItemEditor : Control
    {
        private AppBarButton? _newContentButton;
        private AppBarButton? _saveContentButton;
        private TextBox? _titleTextBox;
        private TextBox? _descriptionTextBox;
        private DataItemTagListEditor? _listView;
        private TextBlock? _referencedFilePathTextBlock;
        private Button? _selectReferencedFileButton;
        private Button? _removeReferenceButton;

        public delegate void RequestSaveContentEventHandler(object sender, DataItem dataItem);
        public event RequestSaveContentEventHandler? RequestSaveContent;

        public DataItem EditedDataItem
        {
            get
            {
                _editedDataItem.Title = _titleTextBox?.Text ?? string.Empty;
                _editedDataItem.Description = _descriptionTextBox?.Text ?? string.Empty;
                _editedDataItem.ItemTags = _listView?.ItemTags ?? [];
                _editedDataItem.RefPath = _referencedFilePathTextBlock?.Text ?? string.Empty;

                return _editedDataItem;
            }
            set
            {
                _editedDataItem = value;
                UpdateUI();
            }
        }
        private DataItem _editedDataItem = new() { ID = -1 };

        public DataItemEditor()
        {
            DefaultStyleKey = typeof(DataItemEditor);

            this.Loaded += DataItemEditor_Loaded;
        }

        private void DataItemEditor_Loaded(object sender, RoutedEventArgs e)
        {
            if (_newContentButton != null)
            {
                _newContentButton.Click += _newContentButton_Click;
            }
            if (_saveContentButton != null)
            {
                _saveContentButton.Click += _saveContentButton_Click;
            }
            if (_titleTextBox != null)
            {
                _titleTextBox.TextChanged += _titleTextBox_TextChanged;
            }
            if (_descriptionTextBox != null)
            {
                _descriptionTextBox.TextChanged += _descriptionTextBox_TextChanged;
            }
            if (_selectReferencedFileButton != null)
            {
                _selectReferencedFileButton.Click += _selectReferencedFileButton_Click;
            }
            if (_removeReferenceButton != null)
            {
                _removeReferenceButton.Click += _removeReferenceButton_Click;
            }

            this.Unloaded += DataItemEditor_Unloaded;
        }

        private void DataItemEditor_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_newContentButton != null)
            {
                _newContentButton.Click -= _newContentButton_Click;
            }
            if (_saveContentButton != null)
            {
                _saveContentButton.Click -= _saveContentButton_Click;
            }
            if (_titleTextBox != null)
            {
                _titleTextBox.TextChanged -= _titleTextBox_TextChanged;
            }
            if (_descriptionTextBox != null)
            {
                _descriptionTextBox.TextChanged -= _descriptionTextBox_TextChanged;
            }
            if (_selectReferencedFileButton != null)
            {
                _selectReferencedFileButton.Click -= _selectReferencedFileButton_Click;
            }
            if (_removeReferenceButton != null)
            {
                _removeReferenceButton.Click -= _removeReferenceButton_Click;
            }
            this.Loaded -= DataItemEditor_Loaded;
            this.Unloaded -= DataItemEditor_Unloaded;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _newContentButton = GetTemplateChild("PART_NewContentButton") as AppBarButton;
            _saveContentButton = GetTemplateChild("PART_SaveContentButton") as AppBarButton;
            _titleTextBox = GetTemplateChild("PART_TitleTextBox") as TextBox;
            _descriptionTextBox = GetTemplateChild("PART_DescriptionTextBox") as TextBox;
            _listView = GetTemplateChild("PART_TagListEditor") as DataItemTagListEditor;
            _referencedFilePathTextBlock = GetTemplateChild("PART_ReferencedFilePathTextBlock") as TextBlock;
            _selectReferencedFileButton = GetTemplateChild("PART_SelectFileButton") as Button;
            _removeReferenceButton = GetTemplateChild("PART_RemoveReferenceButton") as Button;

            if (_saveContentButton != null && _titleTextBox != null)
            {
                _saveContentButton.IsEnabled = _titleTextBox.Text.Length > 0;
            }

            UpdateUI();
        }

        private void _removeReferenceButton_Click(object sender, RoutedEventArgs e)
        {
            _editedDataItem.RefPath = string.Empty;
            if (_referencedFilePathTextBlock != null)
            {
                _referencedFilePathTextBlock.Text = string.Empty;
            }
        }

        private async void _selectReferencedFileButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new Microsoft.UI.Xaml.Window();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            // DO NOT USE Windows.Storage.Pickers since it cant open without filter
            var picker = new Microsoft.Windows.Storage.Pickers.FileOpenPicker(windowId);

            PickFileResult file = await picker.PickSingleFileAsync();
            window.Close();
            if (file != null)
            {
                _editedDataItem.RefPath = file.Path;
                if (_referencedFilePathTextBlock != null)
                {
                    _referencedFilePathTextBlock.Text = file.Path;
                }
            }
        }

        private void _descriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_descriptionTextBox != null)
            {
                _editedDataItem.Description = _descriptionTextBox.Text;
            }
        }

        private void _titleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_titleTextBox != null)
            {
                _editedDataItem.Title = _titleTextBox.Text;
            }
            if (_saveContentButton != null)
            {
                _saveContentButton.IsEnabled = _titleTextBox?.Text.Length > 0;
            }
        }

        private void _saveContentButton_Click(object sender, RoutedEventArgs e)
        {
            RequestSaveContent?.Invoke(this, EditedDataItem);
        }

        private void _newContentButton_Click(object sender, RoutedEventArgs e)
        {
            _editedDataItem = new() { ID = -1 };
        }

        public void UpdateUI()
        {
            if (_titleTextBox == null
                || _descriptionTextBox == null
                || _listView == null
                || _referencedFilePathTextBlock == null
                || _saveContentButton == null) return;
            _titleTextBox.TextChanged -= _titleTextBox_TextChanged;
            _descriptionTextBox.TextChanged -= _descriptionTextBox_TextChanged;
            _titleTextBox.Text = _editedDataItem.Title;
            _descriptionTextBox.Text = _editedDataItem.Description;
            _listView.ItemTags = _editedDataItem.ItemTags;
            _referencedFilePathTextBlock.Text = _editedDataItem.RefPath;
            _titleTextBox.TextChanged += _titleTextBox_TextChanged;
            _descriptionTextBox.TextChanged += _descriptionTextBox_TextChanged;
            _saveContentButton.IsEnabled = _editedDataItem.Title.Length > 0;
        }
    }
}
