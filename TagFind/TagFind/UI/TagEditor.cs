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
using TagFind.Classes.DB;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI
{
    public sealed partial class TagEditor : Control
    {
        private TextBox? _tagNameTextBox;
        private TextBox? _descriptionTextBox;
        private TextBox? _tagSurnamesTextBox;
        private TextBlock? _createdTimeTextBlock;
        private TextBlock? _modifiedTimeTextBlock;
        private LogicChainListEditor? _logicChainListEditor;
        private PropertyListEditor? _propertyListEditor;

        public Tag EditedTag
        {
            get
            {
                Tag tag = _editedTag;
                if (_tagNameTextBox != null)
                {
                    tag.MainName = _tagNameTextBox.Text;
                }
                if (_descriptionTextBox != null)
                {
                    tag.Description = _descriptionTextBox.Text;
                }
                if (_tagSurnamesTextBox != null)
                {
                    tag.Surnames = _tagSurnamesTextBox.Text.SplitIntoSurnames();
                }
                if (_logicChainListEditor != null)
                {
                    tag.LogicChains = _logicChainListEditor.EditedLogicChainList;
                }
                if (_propertyListEditor != null)
                {
                    tag.PropertyItems = _propertyListEditor.EditedPropertyItems;
                }
                return tag;
            }
            set
            {
                _editedTag = value;
                UpdateDataContext();
            }
        }
        private Tag _editedTag = new() { ID = -1 };

        public TagEditor()
        {
            DefaultStyleKey = typeof(TagEditor);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _tagNameTextBox = GetTemplateChild("PART_TagNameTextBox") as TextBox;
            _descriptionTextBox = GetTemplateChild("PART_DescriptionTextBox") as TextBox;
            _tagSurnamesTextBox = GetTemplateChild("PART_SurnamesTextBox") as TextBox;
            _createdTimeTextBlock = GetTemplateChild("PART_CreatedTimeTextBlock") as TextBlock;
            _modifiedTimeTextBlock = GetTemplateChild("PART_ModifiedTimeTextBlock") as TextBlock;
            _logicChainListEditor = GetTemplateChild("PART_LogicChainListEditor") as LogicChainListEditor;
            _propertyListEditor = GetTemplateChild("PART_PropertyListEditor") as PropertyListEditor;

            UpdateDataContext();
        }

        private void UpdateDataContext()
        {
            this.DataContext = _editedTag;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_tagNameTextBox != null)
            {
                _tagNameTextBox.Text = _editedTag.MainName;
            }
            if (_descriptionTextBox != null)
            {
                _descriptionTextBox.Text = _editedTag.Description;
            }
            if (_tagSurnamesTextBox != null)
            {
                _tagSurnamesTextBox.Text = _editedTag.Surnames.CombineSurnames();
            }
            if (_createdTimeTextBlock != null)
            {
                _createdTimeTextBlock.Text = _editedTag.CreatedTime.ToString();
            }
            if (_modifiedTimeTextBlock != null)
            {
                _modifiedTimeTextBlock.Text = _editedTag.ModifiedTime.ToString();
            }
            if (_logicChainListEditor != null)
            {
                _logicChainListEditor.EditedLogicChainList = _editedTag.LogicChains;
            }
            if (_propertyListEditor != null)
            {
                _propertyListEditor.EditedPropertyItems = _editedTag.PropertyItems;
            }
        }
    }
}
