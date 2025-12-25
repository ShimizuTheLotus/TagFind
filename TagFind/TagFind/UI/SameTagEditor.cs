using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using TagFind.Classes.DataTypes;
using TagFind.Classes.Webservices.Wikidata;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI;

public sealed partial class SameTagEditor : Control
{
    private AppBarButton? _addAppBarButton;
    private AppBarButton? _removeAppBarButton;
    private Expander? _expander;
    private ListView? _listView;
    private ProgressRing? _searchProgressRing;
    private TextBlock? _searchResultExceptionTextBlock;

    public string SearchString = string.Empty;
    public ObservableCollection<UniTag> SameUnitags = [];

    public SameTagEditor()
    {
        DefaultStyleKey = typeof(SameTagEditor);

        this.Loaded += SameTagEditor_Loaded;
    }

    private void SameTagEditor_Loaded(object sender, RoutedEventArgs e)
    {
        this.Unloaded += SameTagEditor_Unloaded;
        if(_addAppBarButton != null)
        {
            _addAppBarButton.Click += _addAppBarButton_Click;
        }
        if(_removeAppBarButton != null)
        {
            _removeAppBarButton.Click += _removeAppBarButton_Click; ;
        }
        if (_listView != null)
        {
            _listView.SelectionChanged += _listView_SelectionChanged;
        }
    }

    private void SameTagEditor_Unloaded(object sender, RoutedEventArgs e)
    {
        this.Unloaded -= SameTagEditor_Unloaded;

        if (_addAppBarButton != null)
        {
            _addAppBarButton.Click -= _addAppBarButton_Click;
        }
        if (_removeAppBarButton != null)
        {
            _removeAppBarButton.Click -= _removeAppBarButton_Click;
        }
        if (_listView != null)
        {
            _listView.SelectionChanged -= _listView_SelectionChanged;
        }
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _addAppBarButton = GetTemplateChild("PART_AddTagButton") as AppBarButton;
        _removeAppBarButton = GetTemplateChild("PART_RemoveTagButton") as AppBarButton;
        _expander = GetTemplateChild("PART_Expander") as Expander;
        _listView = GetTemplateChild("PART_TagListView") as ListView;
        _searchProgressRing = GetTemplateChild("PART_SearchProgressRing") as ProgressRing;
        _searchResultExceptionTextBlock = GetTemplateChild("PART_SearchResultExceptionTextBlock") as TextBlock;
    }

    private void _removeAppBarButton_Click(object sender, RoutedEventArgs e)
    {
        if(_listView != null)
        {
            var selectedItems = _listView.SelectedItems.Cast<UniTag>().ToList();
            foreach (var item in selectedItems)
            {
                SameUnitags.Remove(item);
            }
            _listView.ItemsSource = null;
            _listView.ItemsSource = SameUnitags;
        }
    }

    private async void _addAppBarButton_Click(object sender, RoutedEventArgs e)
    {
        WikidataUniTagManager wikidataUniTagManager = new();
        if (_listView != null)
        {
            if (_expander != null)
            {
                _expander.IsExpanded = true;
            }
            TagEditor? tagEditor = FindVisualParent<TagEditor>(this);
            string searchString = string.Empty;
            if (tagEditor != null)
            {
                searchString = tagEditor.GetEditedMainName();
            }
            if (_searchProgressRing != null)
            {
                _searchProgressRing.Visibility = Visibility.Visible;
            }
            if (_searchResultExceptionTextBlock != null)
            {
                _searchResultExceptionTextBlock.Visibility = Visibility.Collapsed;
            }
            _listView.ItemsSource = null;
            List<SearchResult>? searchResult = await wikidataUniTagManager.GetSearchResult(searchString);
            if (_searchProgressRing != null)
            {
                _searchProgressRing.Visibility = Visibility.Collapsed;
            }
            if (_searchResultExceptionTextBlock != null)
            {
                if (searchResult == null || searchResult.Count == 0)
                {
                    _searchResultExceptionTextBlock.Visibility = Visibility.Visible;
                }

                if (searchResult == null)
                {
                    _searchResultExceptionTextBlock.Text = "Failed to get result. This might be network issue or decoding failed.";
                }
                else
                {
                    _searchResultExceptionTextBlock.Text = "No search result";
                }
            }
            _listView.ItemsSource = searchResult;
        }
    }

    private void _listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_listView?.SelectedItem != null)
        {
            if(_listView.SelectedItem is SearchResult selectedResult)
            {
                // Wikidata UniTag selected
                UniTag uniTag = new()
                {
                    UniTagSourceGUID = "Wikidata",
                    UniqueID = selectedResult.ID
                };
                AddUnitagInfo(uniTag);
            }
            _listView.Items.Clear();
        }
    }

    private void AddUnitagInfo(UniTag uniTag)
    {
        if (_expander != null)
        {
            _expander.IsExpanded = false;
        }

        SameUnitags.Add(uniTag);
    }

    private static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
    {
        while (child != null)
        {
            if (child is T parent)
                return parent;
            child = VisualTreeHelper.GetParent(child);
        }
        return null;
    }
}
