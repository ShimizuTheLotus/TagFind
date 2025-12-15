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
using static TagFind.Classes.Consts.DB.UserDB;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI
{
    public sealed partial class TagsWrapPanel : ItemsControl
    {
        private WrapPanel? _wrapPanel;

        public new IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set
            {
                SetValue(ItemsSourceProperty, value);
                UpdateItems();
            }
        }

        public void UpdateItems()
        {
            if (_wrapPanel == null) return;
            var itemsSource = ItemsSource;
            if (ItemsSource is List<ItemTagTreeItem> items)
            {
                itemsSource = items.ConvertIntoItemTagDataSource();
            }
            _wrapPanel.Children.Clear();
            if (itemsSource != null)
            {
                foreach (object item in itemsSource)
                {
                    if (item is IEnumerable)
                    {
                        FrameworkElement itemContent = CreateItemContent(item);
                        _wrapPanel.Children.Add(itemContent);
                    }
                    else if (item is ItemTagDataSource source)
                    {
                        FrameworkElement itemContent = new TagBlock()
                        {
                            TagName = source.TagName,
                            TagID = source.TagID,
                            Margin = new(4, 0, 4, 0)
                        };
                        _wrapPanel.Children.Add(itemContent);
                    }
                }
            }
        }

        private FrameworkElement CreateItemContent(object item)
        {
            if (ItemTemplate != null)
            {
                ContentControl contentControl = new()
                {
                    Content = item,
                    ContentTemplate = ItemTemplate
                };
                return contentControl;
            }
            else
            {
                TagBlock tagBlock = new TagBlock
                {
                    TagName = item.ToString() ?? string.Empty,
                };
                return tagBlock;
            }
        }

        public TagsWrapPanel()
        {
            DefaultStyleKey = typeof(TagsWrapPanel);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _wrapPanel = GetTemplateChild("PART_WrapPanel") as WrapPanel ?? new();

            UpdateItems();
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TagBlock();
        }
    }
}
