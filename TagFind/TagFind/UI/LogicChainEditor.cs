using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TagFind.Classes.DataTypes;
using TagFind.Classes.DB;
using TagFind.Interfaces;
using Windows.ApplicationModel.Contacts;
using Windows.UI;
using static TagFind.UI.TagSuggestPopupContent;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI
{
    public sealed partial class LogicChainEditor : Control
    {
        private WrapPanel? _wrapPanel;
        private TextBox? _inputTextBox;

        private Popup _popup = new();

        private TagSuggestPopupContent? _tagSuggestPopup;
        private PropertySuggestPopupContent? _propertySuggestPopup;

        private bool _isSelectingTag = true;

        public LogicChain EditedLogicChain
        {
            get
            {
                if (_editedLogicChain.LogicChainData.Count > 0)
                {
                    _editedLogicChain.LogicChainData[0].ParentPropertyItemID = -1;
                }
                return _editedLogicChain;
            }
            set
            {
                _editedLogicChain = value;
                UpdateUI();
            }
        }
        private LogicChain _editedLogicChain = new();
        public LogicChainEditor()
        {
            DefaultStyleKey = typeof(LogicChainEditor);

            this.Loaded += LogicChainEditor_Loaded;
        }

        private void LogicChainEditor_Loaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded += LogicChainEditor_Unloaded;

            if (_inputTextBox != null)
            {
                _inputTextBox.TextChanged += _inputAutoSuggestBox_TextChanged;
                _inputTextBox.PreviewKeyDown += _inputAutoSuggestBox_KeyDown;
            }
            if (_tagSuggestPopup != null)
            {
                _tagSuggestPopup.TagSelected += _tagSuggestPopup_TagSelected;
            }
            if (_propertySuggestPopup != null)
            {
                _propertySuggestPopup.PropertySelected += _propertySuggestPopup_PropertySelected;
            }
        }

        private void LogicChainEditor_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_inputTextBox != null)
            {
                _inputTextBox.TextChanged -= _inputAutoSuggestBox_TextChanged;
                _inputTextBox.PreviewKeyDown -= _inputAutoSuggestBox_KeyDown;
            }
            if (_tagSuggestPopup != null)
            {
                _tagSuggestPopup.TagSelected -= _tagSuggestPopup_TagSelected;
            }
            if (_propertySuggestPopup != null)
            {
                _propertySuggestPopup.PropertySelected -= _propertySuggestPopup_PropertySelected;
            }
            //this.Loaded -= LogicChainEditor_Loaded;
            this.Unloaded -= LogicChainEditor_Unloaded;
        }

        ~LogicChainEditor()
        {
            DisposeTagSuggestPopup();
            DisposePropertySuggestPopup();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _wrapPanel = GetTemplateChild("PART_LogicChainWrapPanel") as WrapPanel;
            _inputTextBox = GetTemplateChild("PART_InputTextBox") as TextBox;
            _popup = (GetTemplateChild("PART_SuggestPopup") as Popup)!;

            UpdateUI();
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            DisposeTagSuggestPopup();
            DisposePropertySuggestPopup();
        }

        private void UpdateUI()
        {
            if (_wrapPanel == null) return;
            _wrapPanel.Children.Clear();
            foreach (LogicChainItem item in _editedLogicChain.LogicChainData)
            {
                if (item.ParentDataItemID != -1)
                {
                    LogicChainArrow arrow = new()
                    {
                        Text = item.ParentPropertyItemName
                    };
                    _wrapPanel.Children.Add(arrow);
                }
                TagBlock tagBlock = new()
                {
                    TagName = item.OnChainTagName,
                    IsAnyTag = item.OnChainTagID == -1
                };
                int index = _wrapPanel.Children.Count;
                _wrapPanel.Children.Insert(index, tagBlock);
            }
            _wrapPanel.Children.Add(_inputTextBox ?? new TextBox());
        }

        private void _propertySuggestPopup_PropertySelected(object sender, PropertyItem selectedProperty)
        {
            _popup.IsOpen = false;
            DisposePropertySuggestPopup();
            LogicChainAddProperty(selectedProperty);
            if (_inputTextBox != null)
            {
                _inputTextBox.Text = string.Empty;
                if (_propertySuggestPopup != null)
                {
                    _propertySuggestPopup.ItemsSource = new ObservableCollection<PropertyItem>();
                }
                //_inputTextBox.Focus(FocusState.Keyboard);
            }
        }

        private void _tagSuggestPopup_TagSelected(object sender, Tag selectedTag)
        {
            _popup.IsOpen = false;
            DisposeTagSuggestPopup();
            LogicChainAddTag(selectedTag);
            if (_inputTextBox != null)
            {
                _inputTextBox.Text = string.Empty;
                if (_tagSuggestPopup != null)
                {
                    _tagSuggestPopup.ItemsSource = new ObservableCollection<Tag>();
                }
                //_inputTextBox.Focus(FocusState.Keyboard);
            }
        }

        public void LogicChainAddTag(Tag tag)
        {
            TagBlock tagBlock = new()
            {
                TagName = tag.MainName,
                CornerRadius = new CornerRadius(4, 4, 4, 4),
                Margin = new Thickness(4, 4, 4, 4),
                Padding = new Thickness(8, 4, 8, 4),
                IsAnyTag = tag.ID == -1
            };
            if (_wrapPanel == null || _inputTextBox == null) return;
            _wrapPanel.Children.Insert(_wrapPanel.Children.IndexOf(_inputTextBox), tagBlock);
            LogicChainItem logicChainItem = EditedLogicChain.LogicChainData.Count == 0
                ? new()
                : EditedLogicChain.LogicChainData[^1];
            logicChainItem.OnChainTagID = tag.ID;
            logicChainItem.OnChainTagName = tag.MainName;
            if (EditedLogicChain.LogicChainData.Count == 0)
            {
                logicChainItem.ParentDataItemID = -1;
                EditedLogicChain.LogicChainData.Add(logicChainItem);
            }
        }

        public void LogicChainAddProperty(PropertyItem item)
        {
            LogicChainArrow logicChainArrow = new();
            logicChainArrow.Text = item.PropertyName;
            if (_wrapPanel == null || _inputTextBox == null) return;
            _wrapPanel.Children.Insert(_wrapPanel.Children.IndexOf(_inputTextBox), logicChainArrow);
            LogicChainItem logicChainItem = new()
            {
                ParentPropertyItemID = item.ID,
                ParentPropertyItemName = item.PropertyName,
                OnChainTagID = -1
            };
            EditedLogicChain.LogicChainData.Add(logicChainItem);
        }

        private void _inputAutoSuggestBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_wrapPanel == null) return;

            if (_wrapPanel != null
                && _inputTextBox != null)
            {
                int inputIndex = _wrapPanel.Children.IndexOf(_inputTextBox);
                if (inputIndex > 0)
                {
                    var item = _wrapPanel.Children[inputIndex - 1];
                    // Delete from LogicChain property.
                    if (item is TagBlock)
                    {
                        _isSelectingTag = false;
                    }
                    else if (item is LogicChainArrow)
                    {
                        _isSelectingTag = true;
                    }
                }
                if (inputIndex == 0)
                {
                    _isSelectingTag = true;
                }
            }

            if (_isSelectingTag)
            {
                GetTagSuggest();
            }
            else
            {
                GetPropertySuggest();
            }
        }

        private void _inputAutoSuggestBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Back)
            {
                if (_wrapPanel != null
                    && _inputTextBox != null
                    && _inputTextBox.Text.Length == 0)
                {
                    int inputIndex = _wrapPanel.Children.IndexOf(_inputTextBox);
                    if (inputIndex > 0)
                    {
                        var item = _wrapPanel.Children[inputIndex - 1];
                        // Delete from LogicChain property.
                        if (item is TagBlock)
                        {
                            if (EditedLogicChain.LogicChainData.Count > 1)
                            {
                                EditedLogicChain.LogicChainData[^1].OnChainTagID = -1;
                                EditedLogicChain.LogicChainData[^1].OnChainTagName = string.Empty;
                            }
                            // The first one can not any ancestor with a property.
                            // I don't know why the WPF version can work with this mistake. At least I fixed it in WinUI version.
                            else
                            {
                                if (EditedLogicChain.LogicChainData.Count > 0)
                                {
                                    EditedLogicChain.LogicChainData.RemoveAt(EditedLogicChain.LogicChainData.Count - 1);
                                }
                            }
                        }
                        else if (item is LogicChainArrow)
                        {
                            if (EditedLogicChain.LogicChainData.Count > 0)
                            {
                                EditedLogicChain.LogicChainData.RemoveAt(EditedLogicChain.LogicChainData.Count - 1);
                            }
                        }
                        _wrapPanel.Children.RemoveAt(inputIndex - 1);
                    }
                    _popup.IsOpen = false;
                }
            }
            if (e.Key == Windows.System.VirtualKey.Tab)
            {
                // Tag select mode.
                if (_isSelectingTag)
                {
                    if (_tagSuggestPopup != null)
                    {
                        Tag? focusedTag = _tagSuggestPopup.GetKeyBoardFocusedTag();
                        if (focusedTag != null)
                        {
                            _tagSuggestPopup_TagSelected(this, focusedTag);
                        }
                        else
                        {
                            if (_tagSuggestPopup.ItemsSource is List<Tag> tags
                                && tags.Count > 0)
                            {
                                _tagSuggestPopup_TagSelected(this, tags[0]);
                                _popup.IsOpen = false;
                                GetPropertySuggest();
                            }
                        }
                        DisposeTagSuggestPopup();
                    }
                }
                // Property select mode.
                else
                {
                    if (_propertySuggestPopup != null)
                    {
                        PropertyItem? focusedItem = _propertySuggestPopup.GetKeyBoardFocusedPropertyItem();
                        if (focusedItem != null)
                        {
                            _propertySuggestPopup_PropertySelected(this, focusedItem);
                        }
                        else
                        {
                            if (_propertySuggestPopup.ItemsSource is List<PropertyItem> propertyItems
                                && propertyItems.Count > 0)
                            {
                                _propertySuggestPopup_PropertySelected(this, propertyItems[0]);
                                _popup.IsOpen = false;
                                GetTagSuggest();
                            }
                        }
                        DisposePropertySuggestPopup();
                    }
                }
                _isSelectingTag = !_isSelectingTag;
                e.Handled = true;
                _inputTextBox?.Focus(FocusState.Keyboard);
            }
            else if (e.Key == Windows.System.VirtualKey.Down)
            {
                if (_popup != null)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        if (_isSelectingTag)
                        {
                            if (_tagSuggestPopup != null)
                            {
                                if (_tagSuggestPopup.ItemsSource is List<Tag> tags && tags.Count > 0)
                                {
                                    _tagSuggestPopup.FocusAt(0);
                                }
                            }
                        }
                        else
                        {
                            if (_propertySuggestPopup != null)
                            {
                                if (_propertySuggestPopup.ItemsSource is List<PropertyItem> propertyItems && propertyItems.Count > 0)
                                {
                                    _propertySuggestPopup.FocusAtIndex(0);
                                }
                            }
                        }
                    });
                }
            }
        }

        private async void GetTagSuggest()
        {
            _tagSuggestPopup = new();
            if (_inputTextBox != null)
            {
                DBContentManager? contentManager = GetContentManager();
                if (contentManager != null)
                {
                    string searchString = _inputTextBox.Text + "%" ?? string.Empty;
                    long tagPropertyID = EditedLogicChain.LogicChainData.Count > 1 ? EditedLogicChain.LogicChainData[^1].ParentPropertyItemID : -1;
                    var tags = await contentManager.TagPoolGetTagList(searchString);
                    var tagList = tags.ToList().LogicChainEndWithParentPropertyID(tagPropertyID);
                    _tagSuggestPopup = new();
                    _tagSuggestPopup.ItemsSource = tagList;
                    if (tagList.Count > 0)
                    {
                        _tagSuggestPopup.FocusAt(0);
                    }
                    _popup.Child = _tagSuggestPopup;
                    _popup.XamlRoot = _inputTextBox.XamlRoot;
                    _popup.PlacementTarget = _inputTextBox;
                    _popup.DesiredPlacement = PopupPlacementMode.Bottom;
                    _popup.IsOpen = true;
                }
            }
            _tagSuggestPopup.TagSelected += _tagSuggestPopup_TagSelected;
        }

        private async void GetPropertySuggest()
        {
            _propertySuggestPopup = new();
            if (_inputTextBox != null)
            {
                DBContentManager? contentManager = GetContentManager();
                if (contentManager != null)
                {
                    string searchString = _inputTextBox.Text.Trim() ?? string.Empty;
                    long tagID = EditedLogicChain.LogicChainData.Count > 0 ? EditedLogicChain.LogicChainData[^1].OnChainTagID : -1;
                    Tag tag = await contentManager.TagPoolGetTagByID(tagID);
                    if (searchString == string.Empty)
                    {
                        _propertySuggestPopup.ItemsSource = tag.PropertyItems;
                    }
                    else
                    {
                        IEnumerable<PropertyItem> query = from property in tag.PropertyItems
                                                          where property.PropertyName.Contains(searchString)
                                                          select property;
                        _propertySuggestPopup.ItemsSource = query;
                    }
                    _popup.Child = _propertySuggestPopup;
                    _popup.XamlRoot = _inputTextBox.XamlRoot;
                    _popup.PlacementTarget = _inputTextBox;
                    _popup.DesiredPlacement = PopupPlacementMode.Bottom;
                    _popup.IsOpen = true;
                }
            }
            _propertySuggestPopup.PropertySelected += _propertySuggestPopup_PropertySelected;
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

        private void DisposeTagSuggestPopup()
        {
            try
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    if (_popup != null)
                    {
                        _popup.Child = null;
                        _popup.IsOpen = false;
                    }
                    if (_tagSuggestPopup != null)
                    {
                        _tagSuggestPopup.TagSelected -= _tagSuggestPopup_TagSelected;
                        _tagSuggestPopup = null;
                    }
                });
            }
            catch (ObjectDisposedException)
            {

            }
        }

        private void DisposePropertySuggestPopup()
        {
            try
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    _popup.Child = null;
                    _popup.IsOpen = false;
                    if (_propertySuggestPopup != null)
                    {
                        _propertySuggestPopup.PropertySelected -= _propertySuggestPopup_PropertySelected;
                        _propertySuggestPopup = null;
                    }
                });
            }
            catch (ObjectDisposedException)
            {

            }
        }
    }
}
