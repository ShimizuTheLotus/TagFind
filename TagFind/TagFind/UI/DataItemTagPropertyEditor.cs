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
    public sealed partial class DataItemTagPropertyEditor : Control
    {
        private StackPanel? _stackPanel;
        private TextBlock? _propertyNameTextBlock;
        private DataItemTagGroupEditor? _propertyValueTagGroupEditor;

        public ItemTagTreePropertyItem EditedItemTagTreePropertyItem
        {
            get => _editedItemTagTreePropertyItem;
            set
            {

                _editedItemTagTreePropertyItem = value;
                UpdateUI();
            }
        }
        private ItemTagTreePropertyItem _editedItemTagTreePropertyItem = new();
        public long ParentTagID = -1;

        public DataItemTagPropertyEditor()
        {
            DefaultStyleKey = typeof(DataItemTagPropertyEditor);
        }

        private void DataItemTagPropertyEditor_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_propertyValueTagGroupEditor != null)
            {
                //_propertyValueTagGroupEditor.Focus();
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _stackPanel = GetTemplateChild("PART_StackPanel") as StackPanel;
            _propertyNameTextBlock = GetTemplateChild("PART_PropertyNameTextBlock") as TextBlock;
            _propertyValueTagGroupEditor = GetTemplateChild("PART_PropertyValueTagGroupEditor") as DataItemTagGroupEditor;

            if (_stackPanel != null)
            {
                _stackPanel.GotFocus += DataItemTagPropertyEditor_GotFocus;
            }
            UpdateUI();
        }

        public void UpdateUI()
        {
            if (_propertyNameTextBlock == null
                || _propertyValueTagGroupEditor == null) return;
            _propertyNameTextBlock.Text = _editedItemTagTreePropertyItem.PropertyName;
            _propertyValueTagGroupEditor.ParentTagID = ParentTagID;
            _propertyValueTagGroupEditor.PropertyID = _editedItemTagTreePropertyItem.PropertyID;
            _propertyValueTagGroupEditor.EditedItemTagTreeItems = _editedItemTagTreePropertyItem.Children;
            _propertyValueTagGroupEditor.MinHeight = 60;
            _propertyValueTagGroupEditor.MinWidth = 60;
        }
    }
}
