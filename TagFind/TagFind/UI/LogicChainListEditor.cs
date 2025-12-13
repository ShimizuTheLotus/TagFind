using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TagFind.Classes.DataTypes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI
{
    public sealed partial class LogicChainListEditor : Control
    {
        private AppBarButton? _addLogicChainButton;
        private AppBarButton? _removeLogicChainButton;
        private ListView? _listView;

        public List<LogicChain> EditedLogicChainList
        {
            get
            {
                if (_listView == null) return _editedLogicChainList;
                List<LogicChain> result = [];
                foreach (var item in _listView.Items)
                {
                    if (item is LogicChainEditor editor)
                    {
                        result.Add(editor.EditedLogicChain);
                    }
                }
                _editedLogicChainList = result;
                return _editedLogicChainList;
            }
            set
            {
                _editedLogicChainList = value;
                UpdateUI();
            }
        }
        private List<LogicChain> _editedLogicChainList = [];


        public LogicChainListEditor()
        {
            DefaultStyleKey = typeof(LogicChainListEditor);

            this.Loaded += LogicChainListEditor_Loaded;
        }

        private void LogicChainListEditor_Loaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded += LogicChainListEditor_Unloaded;
            if (_addLogicChainButton != null)
            {
                _addLogicChainButton.Click += _addLogicChainButton_Click;
            }
            if (_removeLogicChainButton != null)
            {
                _removeLogicChainButton.Click += _removeLogicChainButton_Click;
            }

            UpdateUI();
        }

        private void LogicChainListEditor_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_addLogicChainButton != null)
            {
                _addLogicChainButton.Click -= _addLogicChainButton_Click;
            }
            if (_removeLogicChainButton != null)
            {
                _removeLogicChainButton.Click -= _removeLogicChainButton_Click;
            }
            //this.Loaded -= LogicChainListEditor_Loaded;
            this.Unloaded -= LogicChainListEditor_Unloaded;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _addLogicChainButton = GetTemplateChild("PART_AddLogicChainButton") as AppBarButton;
            _removeLogicChainButton = GetTemplateChild("PART_RemoveLogicChainButton") as AppBarButton;
            _listView = GetTemplateChild("PART_ListView") as ListView;
        }

        private void _removeLogicChainButton_Click(object sender, RoutedEventArgs e)
        {
            if (_listView != null && _listView.SelectedIndex != -1)
            {
                _editedLogicChainList.RemoveAt(_listView.SelectedIndex);
                UpdateUI();
            }
        }

        private void _addLogicChainButton_Click(object sender, RoutedEventArgs e)
        {
            _editedLogicChainList.Add(new LogicChain { ChainID = -1 });
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_listView != null)
            {
                _listView.Items.Clear();
                foreach (LogicChain chain in _editedLogicChainList)
                {
                    LogicChainEditor editor = new();
                    editor.EditedLogicChain = chain ?? new();
                    _listView.Items.Add(editor);
                }
            }
        }
    }
}
