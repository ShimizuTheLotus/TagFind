using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using TagFind.Classes.Webservices.Wikidata;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI;

public sealed partial class SameTagEditor : Control
{
    private AppBarButton? _addAppBarButton;
    private AppBarButton? _removeAppBarButton;
    private ListView? _listView;

    public List<string> SameTagList = [];

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
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _addAppBarButton = GetTemplateChild("PART_AddTagButton") as AppBarButton;
        _removeAppBarButton = GetTemplateChild("PART_RemoveTagButton") as AppBarButton;
        _listView = GetTemplateChild("PART_TagListView") as ListView;
    }

    private void _removeAppBarButton_Click(object sender, RoutedEventArgs e)
    {
        if(_listView != null)
        {
            var selectedItems = _listView.SelectedItems.Cast<string>().ToList();
            foreach (var item in selectedItems)
            {
                SameTagList.Remove(item);
            }
            _listView.ItemsSource = null;
            _listView.ItemsSource = SameTagList;
        }
    }

    private async void _addAppBarButton_Click(object sender, RoutedEventArgs e)
    {
        WikidataUniTagManager wikidataUniTagManager = new();
        if (_listView != null)
        {
            _listView.ItemsSource = await wikidataUniTagManager.GetSearchResult("Apple");
        }
    }

}
