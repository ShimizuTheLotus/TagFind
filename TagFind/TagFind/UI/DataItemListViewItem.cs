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
using TagFind.Classes.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI
{
    public sealed partial class DataItemListViewItem : Control
    {
        private Image? _thumbnailImage;
        private TagsWrapPanel? _tagsWrapPanel;
        private Grid? _thumbnailImageOverlay;

        public static readonly DependencyProperty DataItemProperty =
            DependencyProperty.Register(nameof(DataItem),
            typeof(DataItem),
            typeof(DataItemListViewItem),
            new PropertyMetadata(null, OnDataItemChanged));
        public DataItem DataItem
        {
            get => GetValue(DataItemProperty) as DataItem ?? new();
            set
            {
                SetValue(DataItemProperty, value);
            }
        }
        public DataItemListViewItem()
        {
            DefaultStyleKey = typeof(DataItemListViewItem);

            this.Loaded += DataItemListViewItem_Loaded;
        }

        private void DataItemListViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded += DataItemListViewItem_Unloaded;
            if (_thumbnailImage != null)
            {
                _thumbnailImage.PointerEntered += _thumbnailImage_PointerEntered;
                _thumbnailImage.PointerExited += _thumbnailImage_PointerExited;
            }

            UpdateUI();
        }

        private void DataItemListViewItem_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_thumbnailImage != null)
            {
                _thumbnailImage.PointerEntered -= _thumbnailImage_PointerEntered;
                _thumbnailImage.PointerExited -= _thumbnailImage_PointerExited;
            }
            //this.Loaded -= DataItemListViewItem_Loaded;
            this.Unloaded -= DataItemListViewItem_Unloaded;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _thumbnailImage = GetTemplateChild("PART_Image") as Image;
            _tagsWrapPanel = GetTemplateChild("PART_TagsWrapPanel") as TagsWrapPanel;
            _thumbnailImageOverlay = GetTemplateChild("PART_ImageOverlay") as Grid;
        }

        private void _thumbnailImage_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "ImageNormal", true);
        }

        private void _thumbnailImage_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "ImageHover", true);
        }

        private static void OnDataItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (DataItemListViewItem)d;
            control.DataItem = e.NewValue as DataItem ?? new();
            control.UpdateUI();
        }

        private async void UpdateUI()
        {
            if (_thumbnailImage != null)
            {
                var image = await DataItem.RefPath.GetThumbnail(100, 100);
                _thumbnailImage.Source = image;
            }
            if (_tagsWrapPanel != null)
            {
                _tagsWrapPanel.ItemsSource = DataItem.ItemTags;
            }
        }
    }
}
