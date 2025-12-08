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
    public sealed partial class SearchModeSwitcher : Control
    {
        private FontIcon? _currentSearchModeFontIcon;
        private MenuFlyoutItem? _globalSearchModeMenuFlyoutItem;
        private MenuFlyoutItem? _folderSearchModeMenuFlyoutItem;
        private MenuFlyoutItem? _currentLayerSearchModeMenuFlyoutItem;

        public delegate void SearchModeChangedEventHandler(object sender, SearchModeEnum searchMode);
        public event SearchModeChangedEventHandler? SearchModeChanged;

        public SearchModeEnum SearchMode { get; private set; } = SearchModeEnum.Layer;

        public SearchModeSwitcher()
        {
            DefaultStyleKey = typeof(SearchModeSwitcher);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _currentSearchModeFontIcon = GetTemplateChild("PART_CurrentSearchModeFontIcon") as FontIcon;
            _globalSearchModeMenuFlyoutItem = GetTemplateChild("PART_GlobalSearchModeMenuFlyoutItem") as MenuFlyoutItem;
            _folderSearchModeMenuFlyoutItem = GetTemplateChild("PART_FolderSearchModeMenuFlyoutItem") as MenuFlyoutItem;
            _currentLayerSearchModeMenuFlyoutItem = GetTemplateChild("PART_CurrentLayerSearchModeMenuFlyoutItem") as MenuFlyoutItem;

            if (_globalSearchModeMenuFlyoutItem != null)
            {
                _globalSearchModeMenuFlyoutItem.Click += _globalSearchModeMenuFlyoutItem_Click;
            }
            if (_folderSearchModeMenuFlyoutItem != null)
            {
                _folderSearchModeMenuFlyoutItem.Click += _folderSearchModeMenuFlyoutItem_Click;
            }
            if (_currentLayerSearchModeMenuFlyoutItem != null)
            {
                _currentLayerSearchModeMenuFlyoutItem.Click += _currentLayerSearchModeMenuFlyoutItem_Click;
            }
        }

        private void _currentLayerSearchModeMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (SearchMode == SearchModeEnum.Layer) return;
            SearchMode = SearchModeEnum.Layer;
            if (_currentSearchModeFontIcon != null)
            {
                _currentSearchModeFontIcon.Glyph = "\uE81E";
            }
            SearchModeChanged?.Invoke(this, SearchMode);
        }

        private void _folderSearchModeMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (SearchMode == SearchModeEnum.Folder) return;
            SearchMode = SearchModeEnum.Folder;
            if (_currentSearchModeFontIcon != null)
            {
                _currentSearchModeFontIcon.Glyph = "\uE838";
            }
            SearchModeChanged?.Invoke(this, SearchMode);
        }

        private void _globalSearchModeMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (SearchMode == SearchModeEnum.Global) return;
            SearchMode = SearchModeEnum.Global;
            if (_currentSearchModeFontIcon != null)
            {
                _currentSearchModeFontIcon.Glyph = "\uE774";
            }
            SearchModeChanged?.Invoke(this, SearchMode);
        }
    }
}
