using CommunityToolkit.WinUI;
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
using Windows.Devices.Input;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI
{
    public sealed partial class TagSuggestPopupContent : Control
    {
        private ListView? _listView;

        public delegate void TagSelectedEventHandler(object sender, Tag selectedTag);
        public event TagSelectedEventHandler? TagSelected;

        public enum SearchModeEnum
        {
            Parent,// Select tags with specific parent
            Ancestor// Select tags with specific ancestor
        };

        public SearchModeEnum SearchMode = TagSuggestPopupContent.SearchModeEnum.Parent;

        public IEnumerable ItemsSource
        {
            get
            {
                if (_listView == null) return _itemsSource;
                return (IEnumerable)(_listView.ItemsSource ?? Enumerable.Empty<Tag>());
            }
            set
            {
                _itemsSource = value;
                if (_listView != null)
                {
                    _listView.ItemsSource = _itemsSource;
                }
            }
        }

        private IEnumerable _itemsSource = Enumerable.Empty<Tag>();

        public Tag? SelectedTag
        {
            get
            {
                if (_listView == null) return null;
                return (Tag?)_listView.SelectedItem;
            }
        }

        public TagSuggestPopupContent()
        {
            DefaultStyleKey = typeof(TagSuggestPopupContent);
            this.Loaded += TagSuggestPopupContent_Loaded;
        }
        private void TagSuggestPopupContent_Loaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded += TagSuggestPopupContent_Unloaded;
            if (_listView != null)
            {
                _listView.PreviewKeyDown += _listView_PreviewKeyDown;
            }
        }

        private void TagSuggestPopupContent_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_listView != null)
            {
                _listView.PreviewKeyDown -= _listView_PreviewKeyDown;
            }
            //this.Loaded -= TagSuggestPopupContent_Loaded;
            this.Unloaded -= TagSuggestPopupContent_Unloaded;
        }

        private void _listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var focusedElement = FocusManager.GetFocusedElement();
            //if (focusedElement is not ListViewItem focusedItem) return;
            //if (_listView?.Items.IndexOf(focusedItem) == -1) return;
            //var focusState = focusedItem.FocusState;
            //if (focusState != FocusState.Pointer) return;

            if (_listView == null) return;
            if (_listView.SelectedIndex == -1) return;
            Tag tag = _itemsSource.Cast<Tag>().ElementAt(_listView.SelectedIndex);
            TagSelected?.Invoke(this, tag);
            _listView.SelectedIndex = -1;
        }

        private void _listView_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (_listView == null) return;
            if (e.Key == VirtualKey.Enter
                || e.Key == VirtualKey.Tab)
            {
                if (_listView.SelectedIndex != -1)
                {
                    Tag tag = _itemsSource.Cast<Tag>().ElementAt(_listView.SelectedIndex);
                    TagSelected?.Invoke(this, tag);
                    _listView.SelectedIndex = -1;
                }
                e.Handled = true;
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _listView = GetTemplateChild("PART_TagListView") as ListView;
            if (_listView != null)
            {
                _listView.SelectionChanged += _listView_SelectionChanged;
                _listView.ItemsSource = _itemsSource;
            }
        }

        public Tag? GetKeyBoardFocusedTag()
        {
            DependencyObject? focusedElement = FocusManager.GetFocusedElement() as DependencyObject;
            if (focusedElement == null) return null;
            if (VisualTreeHelper.GetParent(focusedElement) != _listView)
                return null;
            int index = _listView.ItemContainerGenerator.IndexFromContainer(focusedElement);
            if (index < 0 && index >= _listView.Items.Count)
            {
                return null;
            }
            if (_listView.ItemsSource is not List<Tag> tags)
            {
                return null;
            }
            else
            {
                return tags[index];
            }
        }

        public void FocusAt(int index)
        {
            if (_listView == null) return;
            // WPF
            //var item = _listView.Items[index];
            //var listViewItem = _listView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
            //listViewItem?.Focus(FocusState.Keyboard);

            // WinUI
            _listView.SelectionChanged -= _listView_SelectionChanged;
            _listView.SelectedIndex = index;
            _listView.SelectionChanged += _listView_SelectionChanged;
        }
    }
}
