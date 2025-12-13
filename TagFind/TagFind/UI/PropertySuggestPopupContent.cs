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
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI
{
    public sealed partial class PropertySuggestPopupContent : Control
    {
        private ListView? _listView;

        public delegate void PropertySelectedEventHandler(object sender, PropertyItem selectedProperty);
        public event PropertySelectedEventHandler? PropertySelected;

        public IEnumerable ItemsSource
        {
            get
            {
                return _itemsSource;
            }
            set
            {
                if (_itemsSource != value)
                {
                    _itemsSource = value;
                    if (_listView != null)
                    {
                        _listView.ItemsSource = value;
                    }
                }
            }
        }
        private IEnumerable _itemsSource = Enumerable.Empty<PropertyItem>();

        public PropertyItem? SelectedProperty
        {
            get
            {
                if (_listView == null) return null;
                return (PropertyItem?)_listView.SelectedItem;
            }
        }

        public PropertySuggestPopupContent()
        {
            DefaultStyleKey = typeof(PropertySuggestPopupContent);

            this.Loaded += PropertySuggestPopupContent_Loaded;
        }

        private void PropertySuggestPopupContent_Loaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded += PropertySuggestPopupContent_Unloaded;
            if (_listView != null)
            {
                _listView.SelectionChanged += _listView_SelectionChanged;
            }
        }

        private void PropertySuggestPopupContent_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_listView != null)
            {
                _listView.SelectionChanged -= _listView_SelectionChanged;
            }
            //this.Loaded -= PropertySuggestPopupContent_Loaded;
            this.Unloaded -= PropertySuggestPopupContent_Unloaded;
        }

        private void _listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_listView == null) return;
            if (_listView.SelectedIndex == -1) return;
            PropertyItem propertyItem = _itemsSource.Cast<PropertyItem>().ElementAt(_listView.SelectedIndex);
            PropertySelected?.Invoke(this, propertyItem);
            _listView.SelectedIndex = -1;
        }


        private void PropertySuggestTab_Loaded(object sender, RoutedEventArgs e)
        {
            if (_listView != null)
            {
                _listView.PreviewKeyDown += _listView_PreviewKeyDown;
            }
        }

        private void _listView_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (_listView == null) return;
            if (e.Key == VirtualKey.Enter
                || e.Key == VirtualKey.Tab)
            {
                if (_listView.SelectedIndex != -1)
                {
                    PropertyItem propertyItem = _itemsSource.Cast<PropertyItem>().ElementAt(_listView.SelectedIndex);
                    PropertySelected?.Invoke(this, propertyItem);
                    _listView.SelectedIndex = -1;
                }
                e.Handled = true;
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _listView = GetTemplateChild("PART_ListView") as ListView;

            if (_listView != null)
            {
                _listView.ItemsSource = _itemsSource;
            }
        }

        public PropertyItem? GetKeyBoardFocusedPropertyItem()
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
            if (_listView.ItemsSource is not List<PropertyItem> items)
            {
                return null;
            }
            else
            {
                return items[index];
            }
        }

        public void FocusAtIndex(int index)
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
