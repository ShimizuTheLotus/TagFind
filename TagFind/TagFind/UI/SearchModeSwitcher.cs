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

        public SearchModeEnum SearchMode
        {
            get => _searchMode;
            set
            {
                _searchMode = value;
                UpdateUI();
            }
        }
        private SearchModeEnum _searchMode = SearchModeEnum.Layer;
        public SearchModeSwitcher()
        {
            DefaultStyleKey = typeof(SearchModeSwitcher);

            this.Loaded += SearchModeSwitcher_Loaded;
        }

        private void SearchModeSwitcher_Loaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded += SearchModeSwitcher_Unloaded;
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

        private void SearchModeSwitcher_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_globalSearchModeMenuFlyoutItem != null)
            {
                _globalSearchModeMenuFlyoutItem.Click -= _globalSearchModeMenuFlyoutItem_Click;
            }
            if (_folderSearchModeMenuFlyoutItem != null)
            {
                _folderSearchModeMenuFlyoutItem.Click -= _folderSearchModeMenuFlyoutItem_Click;
            }
            if (_currentLayerSearchModeMenuFlyoutItem != null)
            {
                _currentLayerSearchModeMenuFlyoutItem.Click -= _currentLayerSearchModeMenuFlyoutItem_Click;
            }
            //this.Loaded -= SearchModeSwitcher_Loaded;
            this.Unloaded -= SearchModeSwitcher_Unloaded;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _currentSearchModeFontIcon = GetTemplateChild("PART_CurrentSearchModeFontIcon") as FontIcon;
            _globalSearchModeMenuFlyoutItem = GetTemplateChild("PART_GlobalSearchModeMenuFlyoutItem") as MenuFlyoutItem;
            _folderSearchModeMenuFlyoutItem = GetTemplateChild("PART_FolderSearchModeMenuFlyoutItem") as MenuFlyoutItem;
            _currentLayerSearchModeMenuFlyoutItem = GetTemplateChild("PART_CurrentLayerSearchModeMenuFlyoutItem") as MenuFlyoutItem;

            UpdateUI();
        }

        private void _currentLayerSearchModeMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (_searchMode == SearchModeEnum.Layer) return;
            _searchMode = SearchModeEnum.Layer;
            if (_currentSearchModeFontIcon != null)
            {
                _currentSearchModeFontIcon.Glyph = "\uE81E";
            }
            SearchModeChanged?.Invoke(this, _searchMode);
        }

        private void _folderSearchModeMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (_searchMode == SearchModeEnum.Folder) return;
            _searchMode = SearchModeEnum.Folder;
            if (_currentSearchModeFontIcon != null)
            {
                _currentSearchModeFontIcon.Glyph = "\uE838";
            }
            SearchModeChanged?.Invoke(this, _searchMode);
        }

        private void _globalSearchModeMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (_searchMode == SearchModeEnum.Global) return;
            _searchMode = SearchModeEnum.Global;
            if (_currentSearchModeFontIcon != null)
            {
                _currentSearchModeFontIcon.Glyph = "\uE774";
            }
            SearchModeChanged?.Invoke(this, _searchMode);
        }

        private void UpdateUI()
        {
            if (_currentSearchModeFontIcon != null)
            {
                switch (_searchMode)
                {
                    case SearchModeEnum.Global:
                        _currentSearchModeFontIcon.Glyph = "\uE774";
                        break;
                    case SearchModeEnum.Folder:
                        _currentSearchModeFontIcon.Glyph = "\uE838";
                        break;
                    case SearchModeEnum.Layer:
                        _currentSearchModeFontIcon.Glyph = "\uE81E";
                        break;
                }
            }
        }
    }
}
