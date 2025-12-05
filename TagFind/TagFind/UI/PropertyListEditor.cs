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
    public sealed partial class PropertyListEditor : Control
    {
        private ListView? _listView;
        private AppBarButton? _createPropertyItemButton;
        private AppBarButton? _deletePropertyItemButton;


        public List<PropertyItem> EditedPropertyItems
        {
            get
            {
                List<PropertyItem> list = _editedPropertyItems;
                if (_listView != null)
                {
                    _editedPropertyItems = [];
                    foreach (PropertyEditor editor in _listView.Items)
                    {
                        _editedPropertyItems.Add(editor.EditedPropertyItem);
                    }
                }
                if (list == null)
                {
                    list = [];
                    _editedPropertyItems = list;
                }
                int i = 0;
                foreach (PropertyItem item in list)
                {
                    if (item.PropertyName != string.Empty)
                    {
                        item.Seq = i++;
                    }
                }
                return list;
            }
            set
            {
                _editedPropertyItems = value;
                UpdateUI();
            }
        }
        private List<PropertyItem> _editedPropertyItems = [];

        public PropertyListEditor()
        {
            DefaultStyleKey = typeof(PropertyListEditor);
            this.Loaded += PropertyItemEditorListView_Loaded;
        }

        private void PropertyItemEditorListView_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _listView = GetTemplateChild("PART_ListView") as ListView;
            _createPropertyItemButton = GetTemplateChild("PART_CreatePropertyItemButton") as AppBarButton;
            _deletePropertyItemButton = GetTemplateChild("PART_DeletePropertyItemButton") as AppBarButton;
            if (_createPropertyItemButton != null)
            {
                _createPropertyItemButton.Click += _createPropertyItemButton_Click;
            }
            if (_deletePropertyItemButton != null)
            {
                _deletePropertyItemButton.Click += _deletePropertyItemButton_Click;
            }
        }

        private void _deletePropertyItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (_listView == null) return;
            if (_listView.SelectedItem == null) return;
            _editedPropertyItems.RemoveAt(_listView.SelectedIndex);
            UpdateUI();
        }

        private void _createPropertyItemButton_Click(object sender, RoutedEventArgs e)
        {
            _editedPropertyItems.Add(new() { ID = -1 });
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_listView == null) return;
            _listView.Items.Clear();
            foreach (PropertyItem propertyItem in _editedPropertyItems ?? [])
            {
                PropertyEditor editor = new();
                editor.EditedPropertyItem = propertyItem ?? new();
                _listView.Items.Add(editor);
            }
        }
    }
}
