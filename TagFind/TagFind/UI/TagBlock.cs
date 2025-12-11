using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TagFind.Interfaces;
using Windows.UI;
using static TagFind.Classes.DataTypes.SearchCondition;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI
{
    public sealed partial class TagBlock : Control
    {
        private TextBlock? _block;
        private Border? _border;

        private DateTime _pointerPressedTime = DateTime.Now;

        public bool IsAnyTag
        {
            get
            {
                return _isAnyTag;
            }
            set
            {
                _isAnyTag = value;
                if (_block != null)
                {
                    _block.Text = value ? "(Any)" : TagName;
                }
            }
        }

        private bool _isAnyTag = false;

        public new CornerRadius CornerRadius
        {
            get
            {
                return _border != null ? _border.CornerRadius : new CornerRadius(0);
            }
            set
            {
                _cornerRadius = value;
                if (_border != null)
                {
                    _border.CornerRadius = value;
                }
            }
        }
        private CornerRadius _cornerRadius = new CornerRadius(0);
        public static readonly DependencyProperty TagNameProperty =
            DependencyProperty.Register(
                nameof(TagName),
                typeof(string),
                typeof(TagBlock),
                new PropertyMetadata(null, OnTagNameChanged));

        public string TagName
        {
            get { return (string)GetValue(TagNameProperty); }
            set { SetValue(TagNameProperty, value); }
        }
        public long TagID
        {
            get => _tagID;
            set => _tagID = value;
        }
        private long _tagID = -1;

        private static void OnTagNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TagBlock tagBlock = (TagBlock)d;
            if (tagBlock._block != null)
            {
                tagBlock._block.Text = e.NewValue as string;
            }
        }

        public TagBlock()
        {
            DefaultStyleKey = typeof(TagBlock);
            this.PointerEntered += TagBlock_PointerEntered;
            this.PointerExited += TagBlock_PointerExited;
            this.PointerPressed += TagBlock_PointerPressed;
            this.PointerReleased += TagBlock_PointerReleased;
            this.Tapped += TagBlock_Tapped;
        }

        private void TagBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Avoid bubble events.
            e.Handled = true;
        }

        public void Clicked()
        {
            if (_tagID == -1) return;

            var page = FindVisualParent<Page>(this);
            if (page is IDatabaseSearchConditionEditablePage iPage)
            {
                iPage.AddConditionAndNavigateToExplorerPage(new TagCondition()
                {
                    TagID = _tagID,
                    TagName = this.TagName
                });
            }
        }

        private void TagBlock_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "PointerOver", true);
            if ((DateTime.Now - _pointerPressedTime).TotalSeconds < 0.2)
            {
                e.Handled = true;
                Clicked();
            }
        }

        private void TagBlock_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Pressed", true);
            _pointerPressedTime = DateTime.Now;
            e.Handled = true;
        }

        private void TagBlock_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", true);
        }

        private void TagBlock_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "PointerOver", true);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _block = GetTemplateChild("PART_TextBlock") as TextBlock ?? new();
            _border = GetTemplateChild("PART_Border") as Border ?? new();
            if (_block != null)
            {
                if (IsAnyTag)
                    _block.Text = "(Any)";
                else
                    _block.Text = TagName;
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
    }
}
