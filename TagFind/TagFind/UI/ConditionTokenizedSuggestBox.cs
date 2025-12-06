using CommunityToolkit.WinUI.Controls;
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
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using TagFind.Classes.DataTypes;
using TagFind.Classes
    ;
using TagFind.Interfaces;
using static TagFind.Classes.DataTypes.SearchCondition;
using TagFind.Classes.DB;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI
{
    public class SearchPseudoTag
    {
        public string MainName { get; set; } = string.Empty;
        public string IconGlyph { get; set; } = "\uE8D2";
    }

    public sealed partial class ConditionTokenizedSuggestBox : Control
    {
        private TokenizingTextBox? _tokenizingTextBox;

        AutoSuggestBox? referencedAutoSuggestBox;

        public ObservableCollection<SearchCondition> SearchConditions = [];

        ObservableCollection<Tag> suggestedTags = [];

        public delegate void RequestSearchEventHandler(object sender, ObservableCollection<SearchCondition> searchConditions, DataItemSearchConfig? config = null);
        public event RequestSearchEventHandler? RequestSearch;

        public ConditionTokenizedSuggestBox()
        {
            DefaultStyleKey = typeof(ConditionTokenizedSuggestBox);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _tokenizingTextBox = GetTemplateChild("PART_TokenizingTextBox") as TokenizingTextBox;

            if (_tokenizingTextBox != null)
            {
                _tokenizingTextBox.KeyDown += _tokenizingTextBox_KeyDown;
                _tokenizingTextBox.TextChanged += _tokenizingTextBox_TextChanged;
                _tokenizingTextBox.TokenDelimiter = "\r\n";
                _tokenizingTextBox.TokenItemAdding += _tokenizingTextBox_TokenItemAdding;
                _tokenizingTextBox.QuerySubmitted += _tokenizingTextBox_QuerySubmitted;
                _tokenizingTextBox.TokenItemRemoved += _tokenizingTextBox_TokenItemRemoved;
            }
        }

        private void _tokenizingTextBox_TokenItemRemoved(TokenizingTextBox sender, object args)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    UpdateConditions();
                    RequestSearch?.Invoke(this, SearchConditions);
                }
                catch { }
            });
        }

        private void UpdateConditions()
        {
            if (_tokenizingTextBox == null) return;

            ObservableCollection<SearchCondition> conditions = [];

            foreach (var item in _tokenizingTextBox.Items)
            {
                if (item is TagCondition tagCon)
                {
                    conditions.Add(tagCon);
                }
                else if (item is TextCondition textCon)
                {
                    conditions.Add(textCon);
                }
                else if (item is Tag tag)
                {
                    if (tag.ID == -1)
                    {
                        TextCondition textCondition = new()
                        {
                            MainName = tag.MainName
                        };
                        conditions.Add(textCondition);
                    }
                    else
                    {
                        TagCondition tagCondition = new()
                        {
                            TagID = tag.ID,
                            TagName = tag.MainName
                        };
                        conditions.Add(tagCondition);
                    }
                }
                else if(item is SearchPseudoTag pseudoTag)
                {
                    TextCondition textCondition = new()
                    {
                        MainName = pseudoTag.MainName
                    };
                    conditions.Add(textCondition);
                }
                //else
                //{
                //    if (item != null)
                //    {
                //        try
                //        {
                //            dynamic dy = item;
                //            if (dy.Text != null)
                //            {
                //                TextCondition textCondition = new()
                //                {
                //                    MainName = dy.Text
                //                };
                //                conditions.Add(textCondition);
                //            }
                //        }
                //        catch
                //        {
                //            TextCondition textCondition = new()
                //            {
                //                MainName = item.ToString() ?? string.Empty
                //            };
                //            conditions.Add(textCondition);
                //        }
                //    }
                //}
            }
            SearchConditions = conditions;
        }

        private void _tokenizingTextBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    UpdateConditions();
                    RequestSearch?.Invoke(this, SearchConditions);
                }
                catch { }
            });
        }

        private void _tokenizingTextBox_TokenItemAdding(TokenizingTextBox sender, TokenItemAddingEventArgs args)
        {
            if (args.TokenText != string.Empty)
            {
                SearchPseudoTag pseudoTag = new()
                {
                    MainName = args.TokenText
                };
                args.Item = pseudoTag;
            }
        }

        private async void _tokenizingTextBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            referencedAutoSuggestBox = sender;
            DBContentManager? contentManager = GetContentManager();
            if (contentManager != null)
            {
                string searchString = sender.Text == string.Empty ? string.Empty : sender.Text + "%";
                if (searchString != string.Empty)
                {
                    suggestedTags = await contentManager.TagPoolGetTagList(searchString);
                    DispatcherQueue.TryEnqueue(async () =>
                    {
                        sender.ItemsSource = suggestedTags;
                    });
                }
            }
        }

        public void ApplyConditions(ObservableCollection<SearchCondition> searchConditions)
        {
            if (_tokenizingTextBox != null)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        SearchConditions = searchConditions;
                        _tokenizingTextBox.ItemsSource = new ObservableCollection<object>(SearchConditions);// Use object type to accept other types of input.
                        RequestSearch?.Invoke(this, SearchConditions);
                    }
                    catch { }
                });
            }
        }

        private void _tokenizingTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            //if (e.Key == Windows.System.VirtualKey.Tab)
            //{
            //    e.Handled = true;
            //    if (referencedAutoSuggestBox != null
            //        && referencedAutoSuggestBox.ItemsSource is ObservableCollection<Tag> objects)
            //    {
            //        if (objects.Count() > 0)
            //        {
            //            if (objects.First() is Tag tag)
            //            {
            //                DispatcherQueue.TryEnqueue(() =>
            //                {
            //                    try
            //                    {
            //                        TagCondition tagCondition = new()
            //                        {
            //                            MainName = tag.MainName,
            //                            TagID = tag.ID
            //                        };
            //                        SearchConditions.Add(tagCondition);
            //                        if (_tokenizingTextBox != null)
            //                        {
            //                            _tokenizingTextBox.ItemsSource = new ObservableCollection<object>(SearchConditions);
            //                            //_tokenizingTextBox.Focus(FocusState.Keyboard);
            //                        }
            //                        referencedAutoSuggestBox.ItemsSource = new List<object>();
            //                        referencedAutoSuggestBox.Text = string.Empty;  
            //                    }
            //                    catch { }
            //                });
            //            }
            //        }
            //    }
            //}
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                if (sender is TokenizingTextBox textBox)
                {
                    if (textBox.Text == string.Empty)
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            try
                            {
                                UpdateConditions();
                                RequestSearch?.Invoke(this, SearchConditions);
                            }
                            catch { }
                        });
                    }
                }
            }
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
    }
}
