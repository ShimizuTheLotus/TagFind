using CommunityToolkit.WinUI.Controls;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TagFind.Classes.DataTypes;
using TagFind.Classes.DB;
using TagFind.Interfaces;
using Windows.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI
{
    public sealed partial class DataItemTagGroupEditor : Control, IDisposable
    {
        private TextBox _editingTextBox = new();
        private WrapPanel? _wrapPanel;
        private Popup _suggestPopup = new();
        private TagSuggestPopupContent _tagSuggestPopup = new();

        private int _insertIndex = -1;

        private bool _isFocusingTextBox = false;
        private bool _isProgramSetFocus = false;

        public List<ItemTagTreeItem> EditedItemTagTreeItems
        {
            get => _itemTagTreeItems;
            set
            {
                _itemTagTreeItems = value;
                UpdateUI();
            }
        }
        public long ParentTagID
        {
            get => _parentTagID;
            set
            {
                _parentTagID = value;
                UpdateRestrictionLogicChains();
            }
        }
        private long _parentTagID = -1;
        public long PropertyID = -1;
        public List<LogicChain> RestrictionLogicChains = [];

        private List<ItemTagTreeItem> _itemTagTreeItems = [];

        public DataItemTagGroupEditor()
        {
            DefaultStyleKey = typeof(DataItemTagGroupEditor);

            this.Loaded += DataItemTagGroupEditor_Loaded;
        }

        private void DataItemTagGroupEditor_Loaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded += DataItemTagGroupEditor_Unloaded;
            _tagSuggestPopup.TagSelected += _tagSuggestPopup_TagSelected;
            if (_wrapPanel != null)
            {
                _wrapPanel.PointerPressed += _wrapPanel_MouseDown;
                _wrapPanel.LostFocus += _wrapPanel_LostFocus;
            }
            _editingTextBox.TextChanged += _editingTextBox_TextChanged;
            _editingTextBox.PreviewKeyDown += _editingTextBox_PreviewKeyDown;


            UpdateRestrictionLogicChains();
            UpdateUI();
        }

        private void DataItemTagGroupEditor_Unloaded(object sender, RoutedEventArgs e)
        {
            _tagSuggestPopup.TagSelected -= _tagSuggestPopup_TagSelected;
            if (_wrapPanel != null)
            {
                _wrapPanel.PointerPressed -= _wrapPanel_MouseDown;
                _wrapPanel.LostFocus -= _wrapPanel_LostFocus;
            }
            _editingTextBox.TextChanged -= _editingTextBox_TextChanged;
            _editingTextBox.PreviewKeyDown -= _editingTextBox_PreviewKeyDown;
            this.Loaded -= DataItemTagGroupEditor_Loaded;
            this.Unloaded -= DataItemTagGroupEditor_Unloaded;
        }

        private void _tagSuggestPopup_TagSelected(object sender, Tag selectedTag)
        {
            DisposeTagSuggestPopup();
            if (_wrapPanel == null) return;
            _editingTextBox.TextChanged -= _editingTextBox_TextChanged;
            _editingTextBox.Text = string.Empty;
            _editingTextBox.TextChanged += _editingTextBox_TextChanged;
            ItemTagTreeItem itemTagTreeItem = new()
            {
                TagID = selectedTag.ID,
                TagName = selectedTag.MainName
            };
            int index = _wrapPanel.Children.IndexOf(_editingTextBox);
            if (index != -1)
            {
                _itemTagTreeItems.Insert(index, itemTagTreeItem);
            }
            _suggestPopup.XamlRoot = this.XamlRoot;
            _suggestPopup.PlacementTarget = _editingTextBox;
            _suggestPopup.DesiredPlacement = PopupPlacementMode.BottomEdgeAlignedLeft;
            _suggestPopup.IsOpen = false;
            UpdateUI();
        }

        private void DisposeTagSuggestPopup()
        {
            try
            {
                _tagSuggestPopup.TagSelected -= _tagSuggestPopup_TagSelected;
                _suggestPopup.IsOpen = false;
                _suggestPopup.Child = null;
            }
            catch { }
        }

        private async void _editingTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DBContentManager? dBContentManager = GetContentManager();
            if (dBContentManager == null) return;
            string searchString = _editingTextBox.Text == string.Empty ? string.Empty : _editingTextBox.Text + "%";
            //_tagSuggestPopup.ItemsSource = dBContentManager.TagPoolGetTagList(searchString).LogicChainEndWithParentPropertyID(PropertyID);
            var ob = await dBContentManager.TagPoolGetTagList(searchString);
            _tagSuggestPopup.ItemsSource = ob.Where(x => x.LogicChains.ContainsPath(RestrictionLogicChains));
            _suggestPopup.XamlRoot = this.XamlRoot;
            _suggestPopup.PlacementTarget = _editingTextBox;
            _suggestPopup.DesiredPlacement = PopupPlacementMode.BottomEdgeAlignedLeft;
            _suggestPopup.Child = _tagSuggestPopup;
            _suggestPopup.IsOpen = true;
            _tagSuggestPopup.TagSelected += _tagSuggestPopup_TagSelected;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _wrapPanel = GetTemplateChild("PART_WrapPanel") as WrapPanel;

            _suggestPopup = new();
            _suggestPopup.IsOpen = false;
        }

        private void _editingTextBox_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                _isFocusingTextBox = false;
                _wrapPanel?.Children.Remove(_editingTextBox);
                _suggestPopup.IsOpen = false;
                e.Handled = true;
            }
            else if (e.Key == Windows.System.VirtualKey.Tab)
            {

            }
        }

        private void _wrapPanel_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_isProgramSetFocus)
            {
                _isProgramSetFocus = false;
                _editingTextBox.Focus(FocusState.Keyboard);
                return;
            }
            if (_wrapPanel == null
                || _editingTextBox == null) return;
            if (_isFocusingTextBox)
            {
                _isFocusingTextBox = true;
                return;
            }

            if (!_suggestPopup.IsOpen)
            {
                var element = FocusManager.GetFocusedElement() as TextBox;
                if (_editingTextBox != element)
                {
                    _wrapPanel.Children.Remove(_editingTextBox);
                }
            }
            else
                _suggestPopup.IsOpen = false;
        }

        private void _wrapPanel_MouseDown(object sender, PointerRoutedEventArgs e)
        {
            if (_wrapPanel == null) return;
            if (e.OriginalSource is Popup) return;
            var point = e.GetCurrentPoint(this);
            if (point.Properties.IsLeftButtonPressed)
            {
                FindInsertPosition(point.Position);
                if (_insertIndex >= 0)
                {
                    if (_editingTextBox != null)
                    {
                        _wrapPanel.Children.Remove(_editingTextBox);
                    }

                    // Insert
                    if (_insertIndex >= 0 && _insertIndex <= _wrapPanel.Children.Count)
                    {
                        _wrapPanel.Children.Insert(_insertIndex, _editingTextBox);
                    }
                    else
                    {
                        _wrapPanel.Children.Add(_editingTextBox);
                    }
                    _isProgramSetFocus = true;
                    _editingTextBox?.Focus(FocusState.Keyboard);
                }
                e.Handled = true;
            }
        }

        private void FindInsertPosition(Windows.Foundation.Point clickPosition)
        {
            _insertIndex = -1;
            if (_wrapPanel == null) return;

            if (_wrapPanel.Children.Count == 0)
            {
                _insertIndex = 0;
                return;
            }

            // Measure to get correct info
            _wrapPanel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            _wrapPanel.Arrange(new Rect(0, 0, _wrapPanel.DesiredSize.Width, _wrapPanel.DesiredSize.Height));

            double currentLineTop = 0;
            double currentLineBottom = 0;
            double currentLineRight = 0;
            bool foundInLine = false;

            for (int i = 0; i < _wrapPanel.Children.Count; i++)
            {
                var child = _wrapPanel.Children[i] as FrameworkElement;
                if (child == null) continue;

                // Get edges of child elements, including margin
                //var childBounds = new Rect(
                //    child.TranslatePoint(new Point(0, 0), _wrapPanel),
                //    new Size(child.ActualWidth, child.ActualHeight)
                //);
                var transform = child.TransformToVisual(_wrapPanel);
                var childOrigin = transform.TransformPoint(new Windows.Foundation.Point(0, 0));
                var childBounds = new Windows.Foundation.Rect(
                    childOrigin,
                    new Windows.Foundation.Size(child.ActualWidth, child.ActualHeight)
                );

                // Judge edges to include margin
                //var childRect = new Rect(
                //    childBounds.X - child.Margin.Left,
                //    childBounds.Y - child.Margin.Top,
                //    childBounds.Width + child.Margin.Left + child.Margin.Right,
                //    childBounds.Height + child.Margin.Top + child.Margin.Bottom
                //);
                var margin = child.Margin;
                var childRect = new Windows.Foundation.Rect(
                    childBounds.X - margin.Left,
                    childBounds.Y - margin.Top,
                    childBounds.Width + margin.Left + margin.Right,
                    childBounds.Height + margin.Top + margin.Bottom
                );

                // New line
                if (childRect.Top > currentLineBottom || i == 0)
                {
                    currentLineTop = childRect.Top;
                    currentLineBottom = childRect.Bottom;
                    currentLineRight = 0;
                    foundInLine = false;
                }

                // Clicked before this element
                if (!foundInLine &&
                    clickPosition.Y >= currentLineTop &&
                    clickPosition.Y <= currentLineBottom &&
                    clickPosition.X < childRect.Left)
                {
                    _insertIndex = i;
                    return;
                }

                // If clicked on this element
                if (childRect.Contains(clickPosition))
                {
                    // Clicked this element
                    double twentyPercentWidth = childRect.Width * 0.2;
                    // At left edge
                    if (clickPosition.X < childRect.Left + twentyPercentWidth)
                    {
                        _insertIndex = i;
                    }
                    // At right edge
                    else if (clickPosition.X > childRect.Right - twentyPercentWidth)
                    {
                        _insertIndex = i + 1;
                    }
                    // Clicked on this item
                    else
                    {
                        _insertIndex = -1;
                    }
                    return;
                }

                // Same row
                if (i < _wrapPanel.Children.Count - 1)
                {
                    var nextChild = _wrapPanel.Children[i + 1] as FrameworkElement;
                    if (nextChild != null)
                    {
                        //var nextChildBounds = new Rect(
                        //    nextChild.TranslatePoint(new Point(0, 0), _wrapPanel),
                        //    new Size(nextChild.ActualWidth, nextChild.ActualHeight)
                        //);
                        var nextChildBounds = new Windows.Foundation.Rect(
                            childOrigin,
                            new Windows.Foundation.Size(nextChild.ActualWidth, nextChild.ActualHeight)
                        );

                        var nextChildRect = new Rect(
                            nextChildBounds.X - nextChild.Margin.Left,
                            nextChildBounds.Y - nextChild.Margin.Top,
                            nextChildBounds.Width + nextChild.Margin.Left + nextChild.Margin.Right,
                            nextChildBounds.Height + nextChild.Margin.Top + nextChild.Margin.Bottom
                        );

                        // New row
                        if (nextChildRect.Top > currentLineBottom)
                        {
                            if (clickPosition.Y >= currentLineTop &&
                                clickPosition.Y <= currentLineBottom &&
                                clickPosition.X > childRect.Right)
                            {
                                _insertIndex = i + 1;
                                return;
                            }
                        }
                        else if (clickPosition.X > childRect.Right &&
                                 clickPosition.X < nextChildRect.Left &&
                                 clickPosition.Y >= currentLineTop &&
                                 clickPosition.Y <= currentLineBottom)
                        {
                            _insertIndex = i + 1;
                            return;
                        }
                    }
                }

                currentLineRight = Math.Max(currentLineRight, childRect.Right);
                currentLineBottom = Math.Max(currentLineBottom, childRect.Bottom);
                foundInLine = true;
            }

            // After last row
            if (clickPosition.Y > currentLineBottom)
            {
                _insertIndex = _wrapPanel.Children.Count;
                return;
            }

            // After last one in this row
            if (clickPosition.X > currentLineRight &&
                clickPosition.Y >= currentLineTop &&
                clickPosition.Y <= currentLineBottom)
            {
                _insertIndex = _wrapPanel.Children.Count;
            }
        }

        public void UpdateUI()
        {
            if (_wrapPanel == null) return;
            _wrapPanel.Children.Clear();
            foreach (ItemTagTreeItem item in _itemTagTreeItems)
            {
                DataItemTagEditor editor = new();
                editor.EditedTagTreeItem = item;
                _wrapPanel.Children.Add(editor);
            }
        }
        private async void UpdateRestrictionLogicChains()
        {
            DBContentManager? dBContentManager = GetContentManager();
            if (dBContentManager == null) return;
            RestrictionLogicChains = await dBContentManager.TagDataGetRestrictionLogicChainsOfPropertyItem(PropertyID, _parentTagID);
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

        private DBContentManager? GetContentManager()
        {
            Page? currentPage = FindVisualParent<Page>(this);
            if (currentPage == null)
            {
                return null;
            }
            if (currentPage is IDBContentAccessiblePage _currentPage)
            {
                return _currentPage.ContentManager;
            }
            return null;
        }

        public void Dispose()
        {
            DisposeTagSuggestPopup();
        }
    }
}