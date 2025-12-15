using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TagFind.Classes.DataTypes;
using TagFind.Classes.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI
{
    public sealed partial class DataItemListView : Control
    {
        private ListView? _listView;

        public delegate void DataItemEventHandler(object sender, DataItem dataItem);

        public event DataItemEventHandler? RequestOpenDataItemDetail;

        public event DataItemEventHandler? RequestOpenDataItemAsFolder;

        public ObservableCollection<DataItem> DataItemCollection
        {
            get
            {
                return _dataItemCollection;
            }
            set
            {
                if (_dataItemCollection != value)
                {
                    _dataItemCollection = value;
                    UpdateUI();
                }
            }
        }
        private ObservableCollection<DataItem> _dataItemCollection = [];

        public bool AllowSelecting
        {
            get => _allowSelecting;
            set
            {
                _allowSelecting = value;
                if (value)
                {
                    if (_listView != null)
                    {
                        _listView.IsItemClickEnabled = false;
                        _listView.SelectionMode = ListViewSelectionMode.Multiple;
                    }
                }
                else
                {
                    if (_listView != null)
                    {
                        _listView.IsItemClickEnabled = true;
                        _listView.SelectionMode = ListViewSelectionMode.None;
                    }
                }
            }
        }
        private bool _allowSelecting = false;

        public object SelectedItems => _listView?.SelectedItems ?? [];

        public DataItemListView()
        {
            DefaultStyleKey = typeof(DataItemListView);

            this.Loaded += DataItemListView_Loaded;
        }

        private void DataItemListView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded += DataItemListView_Unloaded;
            if (_listView != null)
            {
                _listView.ItemClick += _listView_ItemClick;
                _listView.DoubleTapped += _listView_DoubleTapped;
                _listView.Tapped += _listView_Tapped;
                _listView.RightTapped += _listView_RightTapped;
            }
        }

        private void DataItemListView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_listView != null)
            {
                _listView.ItemClick -= _listView_ItemClick;
                _listView.DoubleTapped -= _listView_DoubleTapped;
                _listView.Tapped -= _listView_Tapped;
                _listView.RightTapped -= _listView_RightTapped;
            }
            //this.Loaded -= DataItemListView_Loaded;
            this.Unloaded -= DataItemListView_Unloaded;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _listView = GetTemplateChild("PART_ListView") as ListView;

            if (_listView != null)
            {
                _listView.IsItemClickEnabled = true;
                _listView.SelectionMode = ListViewSelectionMode.None;
                _listView.IsDoubleTapEnabled = true;
                _listView.IsTapEnabled = true;
            }

            UpdateUI();
        }

        private void _listView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {

        }

        private void _listView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_allowSelecting) return;
            var originalSource = e.OriginalSource as FrameworkElement;
            if (originalSource == null) return;
            if (_listView == null) return;

            var listViewItem = FindParent<ListViewItem>(originalSource);
            if (listViewItem != null)
            {
                var tem = _listView.ItemFromContainer(listViewItem);

                if (tem is DataItem dataItem)
                {
                    if (e.OriginalSource is Image)
                    {
                        RequestOpenDataItemDetail?.Invoke(this, dataItem);
                    }
                    else
                    {
                        RequestOpenDataItemAsFolder?.Invoke(this, dataItem);
                    }
                }
            }

            e.Handled = true;
        }

        private void _listView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {

        }

        private void _listView_ItemClick(object sender, ItemClickEventArgs e)
        {
            //if (e.ClickedItem is DataItem item)
            //{
            //    RequestOpenDataItemDetail?.Invoke(this, item);
            //}
        }

        private void UpdateUI()
        {
            if (_listView != null)
            {
                _listView.ItemsSource = _dataItemCollection;
            }
        }

        private T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T tParent)
                    return tParent;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
}
