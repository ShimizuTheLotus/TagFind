using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI;

[ContentProperty(Name = nameof(Children))]
public sealed partial class HeaderAreaGrid : Control
{
    private Grid? _headerGrid;
    private TextBlock? _headerTextBlock;
    private Grid? _bodyGrid;
    private Grid _childrenTempGrid = new();

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(
            nameof(Header),
            typeof(string),
            typeof(HeaderAreaGrid),
            new PropertyMetadata(string.Empty, OnHeaderChanged));
    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HeaderAreaGrid panel && panel._headerTextBlock != null)
        {
            panel._headerTextBlock.Text = (string)e.NewValue;
        }
    }

    public UIElementCollection Children => _bodyGrid != null? _bodyGrid.Children : _childrenTempGrid.Children;
    public HeaderAreaGrid()
    {
        DefaultStyleKey = typeof(HeaderAreaGrid);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _headerGrid = GetTemplateChild("PART_HeaderGrid") as Grid;
        _headerTextBlock = GetTemplateChild("PART_HeaderTextBlock") as TextBlock;
        _bodyGrid = GetTemplateChild("PART_BodyGrid") as Grid;

        if (_headerTextBlock != null)
        {
            _headerTextBlock.Text = Header;
        }

        if (_bodyGrid != null)
        {
            foreach (UIElement child in _childrenTempGrid.Children.ToList())
            {
                _childrenTempGrid.Children.Remove(child);
                _bodyGrid.Children.Add(child);
            }
        }
    }
}
