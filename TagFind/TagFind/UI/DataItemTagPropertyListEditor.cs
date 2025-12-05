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
    public sealed partial class DataItemTagPropertyListEditor : Control
    {
        private StackPanel? _listView;

        public List<ItemTagTreePropertyItem> EditedItemTagTreePropertyItem
        {
            get
            {
                if (_listView == null) return _editedItemTagTreePropertyItem;
                _editedItemTagTreePropertyItem.Clear();
                foreach (DataItemTagPropertyEditor editor in _listView.Children)
                {
                    _editedItemTagTreePropertyItem.Add(editor.EditedItemTagTreePropertyItem);
                }
                return _editedItemTagTreePropertyItem;
            }
            set
            {
                _editedItemTagTreePropertyItem = value;
                UpdateUI();
            }
        }
        private List<ItemTagTreePropertyItem> _editedItemTagTreePropertyItem = new();
        public long ParentTagID = -1;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _listView = GetTemplateChild("PART_ListView") as StackPanel;
            UpdateUI();
        }

        public DataItemTagPropertyListEditor()
        {
            DefaultStyleKey = typeof(DataItemTagPropertyListEditor);
        }

        public void UpdateUI()
        {
            if (_listView == null) return;
            _listView.Children.Clear();
            foreach (ItemTagTreePropertyItem propertyItem in _editedItemTagTreePropertyItem)
            {
                DataItemTagPropertyEditor editor = new();
                editor.ParentTagID = ParentTagID;
                editor.EditedItemTagTreePropertyItem = propertyItem;
                _listView.Children.Add(editor);
            }
        }
    }
}
