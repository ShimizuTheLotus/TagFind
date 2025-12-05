using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TagFind.Classes.DataTypes;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI
{
    public sealed partial class LogicChainPanel : ItemsControl
    {
        private WrapPanel? _wrapPanel;

        public new IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set
            {
                SetValue(ItemsSourceProperty, value);
                UpdateUI();
            }
        }

        private List<LogicChainItem> _logicChainItems = [];

        public LogicChainPanel()
        {
            DefaultStyleKey = typeof(LogicChainPanel);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _wrapPanel = GetTemplateChild("PART_WrapPanel") as WrapPanel ?? new();
            UpdateUI();
        }

        public void UpdateUI()
        {
            if (_wrapPanel == null) return;
            _wrapPanel.Children.Clear();
            if (ItemsSource == null)
            {
                return;
            }
            foreach (LogicChainItem item in ItemsSource)
            {
                if (item.ParentDataItemID != -1)
                {
                    LogicChainArrow arrow = new()
                    {
                        Text = item.ParentPropertyItemName
                    };
                    _wrapPanel.Children.Add(arrow);
                }
                TagBlock tagBlock = new()
                {
                    CornerRadius = new CornerRadius(4, 4, 4, 4),
                    Margin = new Thickness(4, 4, 4, 4),
                    TagName = item.OnChainTagName,
                    TagID = item.ID,
                    Padding = new Thickness(8, 4, 8, 4),
                    IsAnyTag = item.TagID == -1
                };
                _wrapPanel.Children.Add(tagBlock);
            }
        }
    }
}
