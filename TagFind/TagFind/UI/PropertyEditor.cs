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
    public sealed partial class PropertyEditor : Control
    {
        private TextBox? _inputTextBox;
        private LogicChainListEditor? _logicChainListEditor;
        private ToggleSwitch? _isContainRelationToggleSwitch;

        public PropertyItem EditedPropertyItem
        {
            get
            {
                PropertyItem item = _editedPropertyItem;
                if (_inputTextBox != null)
                {
                    _editedPropertyItem.PropertyName = _inputTextBox.Text;
                }
                foreach (LogicChain chain in item.RestrictedTagLogicChains)
                {
                    if (chain.LogicChainData.Count > 1)
                    {
                        chain.LogicChainData[^1].OnChainTagID = -1;
                    }
                }
                if (item == null)
                {
                    item = new();
                    _editedPropertyItem = item;
                }
                if (_isContainRelationToggleSwitch != null)
                {
                    _editedPropertyItem.IsContainsRelation = _isContainRelationToggleSwitch.IsOn;
                }

                UpdateUI();
                return item;
            }
            set
            {
                _editedPropertyItem = value;
                this.DataContext = value;
                UpdateUI();
            }
        }

        private PropertyItem _editedPropertyItem = new();

        public PropertyEditor()
        {
            DefaultStyleKey = typeof(PropertyEditor);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _inputTextBox = GetTemplateChild("PART_PropertyNameTextBox") as TextBox;
            _logicChainListEditor = GetTemplateChild("PART_LogicChainListEditor") as LogicChainListEditor;
            _isContainRelationToggleSwitch = GetTemplateChild("PART_IsContainRelationToggleSwitch") as ToggleSwitch;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_inputTextBox != null)
                _inputTextBox.Text = _editedPropertyItem.PropertyName;
            if (_logicChainListEditor != null)
                _logicChainListEditor.EditedLogicChainList = _editedPropertyItem.RestrictedTagLogicChains;
            if (_isContainRelationToggleSwitch != null)
                _isContainRelationToggleSwitch.IsOn = _editedPropertyItem.IsContainsRelation;
        }
    }
}
