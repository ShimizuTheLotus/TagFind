using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TagFind.Classes.DataTypes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI
{
    public sealed partial class DataItemTagListEditor : Control
    {
        private ListView? _listView;
        private AppBarButton? _addDataItemButton;
        private AppBarButton? _deleteDataItemButton;

        public List<ItemTagTreeItem> ItemTags
        {
            get
            {
                if (_listView == null) return _itemTags;
                _itemTags.Clear();
                foreach (DataItemTagEditor editor in _listView.Items)
                {
                    _itemTags.Add(editor.EditedTagTreeItem);
                }
                return _itemTags;
            }
            set
            {
                _itemTags = value;
                UpdateUI();
            }
        }
        private List<ItemTagTreeItem> _itemTags = [];

        public DataItemTagListEditor()
        {
            DefaultStyleKey = typeof(DataItemTagListEditor);

            this.Loaded += DataItemTagListEditor_Loaded;
        }

        private void DataItemTagListEditor_Loaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded += DataItemTagListEditor_Unloaded;
            if (_addDataItemButton != null)
            {
                _addDataItemButton.Click += _addDataItemButton_Click;
            }
            if (_deleteDataItemButton != null)
            {
                _deleteDataItemButton.Click += _deleteDataItemButton_Click;
            }

            UpdateUI();
        }

        private void DataItemTagListEditor_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_addDataItemButton != null)
            {
                _addDataItemButton.Click -= _addDataItemButton_Click;
            }
            if (_deleteDataItemButton != null)
            {
                _deleteDataItemButton.Click -= _deleteDataItemButton_Click;
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _listView = GetTemplateChild("PART_ListView") as ListView;
            _addDataItemButton = GetTemplateChild("PART_AddDataItemButton") as AppBarButton;
            _deleteDataItemButton = GetTemplateChild("PART_DeleteDataItemButton") as AppBarButton;
        }

        private void _deleteDataItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (_listView == null) return;
            if (_listView.SelectedIndex != -1)
            {
                _itemTags.RemoveAt(_listView.SelectedIndex);
                _listView.SelectedIndex = -1;
            }
            UpdateUI();
        }

        private void _addDataItemButton_Click(object sender, RoutedEventArgs e)
        {
            ItemTags.Add(new());
            UpdateUI();
        }

        public void UpdateUI()
        {
            if (_listView == null) return;
            _listView.Items.Clear();
            foreach (ItemTagTreeItem itemTag in _itemTags)
            {
                DataItemTagEditor editor = new();
                editor.EditedTagTreeItem = itemTag;
                editor.ParentPropertyID = -1;
                _listView.Items.Add(editor);
                editor.FocusInputBox();
            }
        }
    }
}
