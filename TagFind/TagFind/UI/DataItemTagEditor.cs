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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI
{
    public sealed partial class DataItemTagEditor : Control
    {
        private TagBlock? _thisTagTagBlock;
        private TextBox? _tagInputTextBox;
        private DataItemTagPropertyListEditor? _listView;
        private Popup? _tagSuggestPopup;
        private TagSuggestPopupContent? _tagSuggestPopupSource;

        public ItemTagTreeItem EditedTagTreeItem
        {
            get
            {
                if (_listView == null) return _editedTag;
                List<ItemTagTreePropertyItem> itemTagTreePropertyItems = [];
                foreach (ItemTagTreePropertyItem propertyItem in _listView.EditedItemTagTreePropertyItem)
                {
                    if (propertyItem.Children.Count > 0)
                    {
                        itemTagTreePropertyItems.Add(propertyItem);
                    }
                }
                _editedTag.PropertyItems = itemTagTreePropertyItems;
                return _editedTag;
            }
            set
            {
                _editedTag = value;
                UpdateUI();
            }
        }

        public long ParentPropertyID = -1;


        private ItemTagTreeItem _editedTag = new() { TagID = -1 };
        public DataItemTagEditor()
        {
            DefaultStyleKey = typeof(DataItemTagEditor);

            this.Loaded += DataItemTagEditor_Loaded;
        }

        private void DataItemTagEditor_Loaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded += DataItemTagEditor_Unloaded;
            if (_thisTagTagBlock != null)
            {
                _thisTagTagBlock.PointerPressed += _thisTagTagBlock_MouseDown;
            }
            if (_tagInputTextBox != null)
            {
                _tagInputTextBox.TextChanged += _tagInputTextBox_TextChanged;
                _tagInputTextBox.PreviewKeyDown += _tagInputTextBox_PreviewKeyDown;
                _tagInputTextBox.LostFocus += _tagInputTextBox_LostFocus;
            }
            if (_tagSuggestPopup != null)
            {
                _tagSuggestPopup.IsOpen = false;
            }

            UpdateUI();
        }

        private void DataItemTagEditor_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_thisTagTagBlock != null)
            {
                _thisTagTagBlock.PointerPressed -= _thisTagTagBlock_MouseDown;
            }
            if (_tagInputTextBox != null)
            {
                _tagInputTextBox.TextChanged -= _tagInputTextBox_TextChanged;
                _tagInputTextBox.PreviewKeyDown -= _tagInputTextBox_PreviewKeyDown;
                _tagInputTextBox.LostFocus -= _tagInputTextBox_LostFocus;

            }
            if (_tagSuggestPopup != null)
            {
                _tagSuggestPopup.IsOpen = false;
            }
            if (_tagSuggestPopupSource != null)
            {
                _tagSuggestPopupSource.PointerExited -= _tagSuggestPopup_PointerExited;
                _tagSuggestPopupSource.TagSelected -= TagSuggestPopup_TagSelected;
            }
            //this.Loaded -= DataItemTagEditor_Loaded;
            this.Unloaded -= DataItemTagEditor_Unloaded;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _thisTagTagBlock = GetTemplateChild("PART_ThisTagTagBlock") as TagBlock;
            _tagInputTextBox = GetTemplateChild("PART_InputTextBox") as TextBox;
            _listView = GetTemplateChild("PART_StackPanel") as DataItemTagPropertyListEditor;
            _tagSuggestPopup = GetTemplateChild("PART_TagSuggestPopup") as Popup;

            if (_tagInputTextBox != null)
            {
                _tagInputTextBox.Visibility = Visibility.Collapsed;
            }
        }

        private void _tagInputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_tagInputTextBox == null || _thisTagTagBlock == null) return;
            _tagInputTextBox.Visibility = Visibility.Collapsed;
            _thisTagTagBlock.Visibility = Visibility.Visible;
        }

        private void _thisTagTagBlock_MouseDown(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            if (_tagInputTextBox == null || _thisTagTagBlock == null) return;
            _tagInputTextBox.Text = _thisTagTagBlock.TagName;
            _thisTagTagBlock.Visibility = Visibility.Collapsed;
            _tagInputTextBox.Visibility = Visibility.Visible;
            FocusInputBox();
        }

        private async void _tagInputTextBox_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                _tagInputTextBox_LostFocus(this, e);
                DisposeTagSuggestPopup();
                e.Handled = true;
            }
            if (e.Key == Windows.System.VirtualKey.Tab)
            {
                if (_tagSuggestPopupSource != null)
                {
                    Tag? focusedTag = _tagSuggestPopupSource.GetKeyBoardFocusedTag();
                    if (focusedTag != null)
                    {
                        TagSuggestPopup_TagSelected(this, focusedTag);
                    }
                    else
                    {
                        if (_tagSuggestPopupSource.ItemsSource is List<Tag> tags
                            && tags.Count > 0)
                        {
                            TagSuggestPopup_TagSelected(this, tags[0]);
                            DisposeTagSuggestPopup();
                        }
                    }
                }
                e.Handled = true;
            }
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                DBContentManager? contentManager = GetContentManager();
                string searchString = string.Empty;
                if (_tagInputTextBox != null && _tagInputTextBox.Text.Trim() != string.Empty)
                {
                    searchString = _tagInputTextBox.Text.Trim();
                }
                ObservableCollection<Tag> searchResults = [];
                if (contentManager != null)
                    searchResults = await contentManager.TagPoolGetTagList(searchString);
                if (searchResults.Count > 0)
                {
                    TagSuggestPopup_TagSelected(this, searchResults[0]);
                }
                // Has no tag matched, ask if user want to create a new one
                else
                {
                    ContentDialog createTagDialog = new();
                    createTagDialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                    createTagDialog.Title = GetLocalizedString("TagNotExists/String");
                    createTagDialog.Content = GetLocalizedString("DoYouWantToCreateThisTagNow/String");
                    createTagDialog.XamlRoot = this.XamlRoot;
                    createTagDialog.PrimaryButtonText = GetLocalizedString("Create/String");
                    createTagDialog.SecondaryButtonText = GetLocalizedString("Cancel/String");
                    createTagDialog.DefaultButton = ContentDialogButton.Primary;
                    createTagDialog.PrimaryButtonClick += CreateTagDialog_PrimaryButtonClick;
                    await createTagDialog.ShowAsync();
                    createTagDialog.PrimaryButtonClick -= CreateTagDialog_PrimaryButtonClick;
                }
                e.Handled = true;
            }
            else if (e.Key == Windows.System.VirtualKey.Down)
            {
                if (_tagSuggestPopup != null
                    && _tagSuggestPopupSource != null
                    && _tagSuggestPopup.IsOpen)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        if (_tagSuggestPopupSource.ItemsSource is List<Tag> tags && tags.Count > 0)
                        {
                            _tagSuggestPopupSource.FocusAt(0);
                        }
                    });
                    e.Handled = true;
                }
            }
        }

        private async void CreateTagDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            DBContentManager? contentManager = GetContentManager();

            if (contentManager != null && _tagInputTextBox != null && _tagInputTextBox.Text.Trim() != string.Empty)
            {
                Tag tag = new() { MainName = _tagInputTextBox.Text.Trim() };
                tag.ID = await contentManager.TagPoolAddUniqueTag(tag);
                TagSuggestPopup_TagSelected(this, tag);
            }
        }

        private async void _tagInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_tagSuggestPopup == null) return;
            if (_tagInputTextBox?.Text == string.Empty) return;
            _tagSuggestPopup.Child = null;
            _tagSuggestPopupSource = new();
            DBContentManager? contentManager = GetContentManager();
            if (contentManager != null)
            {
                string searchString = _tagInputTextBox?.Text + "%" ?? string.Empty;
                _tagSuggestPopupSource.SearchMode = TagSuggestPopupContent.SearchModeEnum.Parent;
                if (ParentPropertyID != -1)
                {
                    List<LogicChain> restrictionLogicChains = [];
                    var obProp = await contentManager.TagPoolGetTagByPropertyID(ParentPropertyID);
                    restrictionLogicChains = obProp.PropertyItems.First(x => x.ID == ParentPropertyID).RestrictedTagLogicChains;
                    if (restrictionLogicChains.Count > 0)
                    {
                        var obTag = await contentManager.TagPoolGetTagList(searchString);
                        _tagSuggestPopupSource.ItemsSource = obTag.Where(x => restrictionLogicChains.ContainsPath(x.LogicChains));
                    }
                }
                else
                {
                    _tagSuggestPopupSource.ItemsSource = await contentManager.TagPoolGetTagList(searchString);
                }
                _tagSuggestPopup.PlacementTarget = _tagInputTextBox;
                _tagSuggestPopup.DesiredPlacement = PopupPlacementMode.BottomEdgeAlignedLeft;
                _tagSuggestPopupSource.TagSelected += TagSuggestPopup_TagSelected;
                _tagSuggestPopupSource.PointerExited += _tagSuggestPopup_PointerExited;
                _tagSuggestPopup.Child = _tagSuggestPopupSource;
                _tagSuggestPopup.IsOpen = true;
            }
        }

        private void _tagSuggestPopup_PointerExited(object sender, RoutedEventArgs e)
        {
            DisposeTagSuggestPopup();
        }

        private void TagSuggestPopup_TagSelected(object sender, Tag selectedTag)
        {
            DisposeTagSuggestPopup();
            if (_thisTagTagBlock == null) return;
            _thisTagTagBlock.TagName = selectedTag.MainName;
            if (_tagInputTextBox == null) return;
            _editedTag.TagName = selectedTag.MainName;
            _editedTag.TagID = selectedTag.ID;
            _tagInputTextBox.Visibility = Visibility.Collapsed;
            _thisTagTagBlock.Visibility = Visibility.Visible;
            List<ItemTagTreePropertyItem> itemTagTreePropertyItems = [];
            foreach (var property in selectedTag.PropertyItems)
            {
                itemTagTreePropertyItems.Add(new()
                {
                    PropertyID = property.ID,
                    PropertyName = property.PropertyName
                });
            }
            _editedTag.PropertyItems = itemTagTreePropertyItems;
            _tagInputTextBox.Text = string.Empty;
            FocusInputBox();
            UpdateUI();
        }

        public void FocusInputBox()
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (_tagInputTextBox == null) return;
                _tagInputTextBox.Focus(FocusState.Keyboard);
                _tagInputTextBox.SelectAll();
            });
        }

        private void DisposeTagSuggestPopup()
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (_tagSuggestPopup == null) return;
                _tagSuggestPopup.Child = null;
                _tagSuggestPopup.IsOpen = false;
                if (_tagSuggestPopupSource != null)
                {
                    _tagSuggestPopupSource.PointerExited -= _tagSuggestPopup_PointerExited;
                    _tagSuggestPopupSource.TagSelected -= TagSuggestPopup_TagSelected;
                    _tagSuggestPopupSource = null;
                }
            });
        }

        public async void UpdateUI()
        {
            if (_thisTagTagBlock == null
                || _listView == null
                || _tagInputTextBox == null
                || _tagSuggestPopup == null) return;
            DBContentManager? contentManager = GetContentManager();
            List<ItemTagTreePropertyItem> itemTagTreePropertyItems = [];
            if (contentManager != null && _editedTag.TagID != -1)
            {
                Tag tag = await contentManager.TagPoolGetTagByID(_editedTag.TagID);
                foreach (var property in tag.PropertyItems)
                {
                    itemTagTreePropertyItems.Add(new()
                    {
                        PropertyID = property.ID,
                        PropertyName = property.PropertyName
                    });
                }
                _listView.EditedItemTagTreePropertyItem = itemTagTreePropertyItems;
            }
            foreach (ItemTagTreePropertyItem propertyItem in _editedTag.PropertyItems)
            {
                if (itemTagTreePropertyItems.Any(x => x.PropertyID == propertyItem.PropertyID))
                {
                    itemTagTreePropertyItems.Where(x => x.PropertyID == propertyItem.PropertyID).First().Children = propertyItem.Children;
                }
            }
            _listView.ParentTagID = EditedTagTreeItem.TagID;
            _listView.EditedItemTagTreePropertyItem = itemTagTreePropertyItems;
            _thisTagTagBlock.TagName = _editedTag.TagName;
            _tagInputTextBox.Text = _editedTag.TagName;
            _tagSuggestPopup.IsOpen = false;
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

        public string GetLocalizedString(string key)
        {
            try
            {
                var resourceLoader = new Microsoft.Windows.ApplicationModel.Resources.ResourceLoader();
                return resourceLoader.GetString(key);
            }
            catch
            {
                return "{Resource Load Failed}";
            }
        }
    }
}
