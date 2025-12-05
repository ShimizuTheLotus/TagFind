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
using TagFind.Classes.DB;
using TagFind.Interfaces;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI
{
    public sealed partial class DatabaseListViewItem : Control
    {
        private AppBarButton? _appBarButton;
        private CommandBarFlyout? _commandBarFlyout;
        private AppBarButton? _removeReferenceAppBarButton;

        public event EventHandler<long>? RequestOpenDatabase;

        public static readonly DependencyProperty DatabaseInfoProperty =
            DependencyProperty.Register(nameof(DatabaseInfo),
            typeof(MetaData),
            typeof(DatabaseListViewItem),
            null);

        public MetaData DatabaseInfo
        {
            get => (MetaData)GetValue(DatabaseInfoProperty);
            set
            {
                SetValue(DatabaseInfoProperty, value);
            }
        }

        public DatabaseListViewItem()
        {
            DefaultStyleKey = typeof(DatabaseListViewItem);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _appBarButton = GetTemplateChild("PART_MoreOptionButton") as AppBarButton;
            _commandBarFlyout = GetTemplateChild("PART_CommandBarFlyout") as CommandBarFlyout;
            _removeReferenceAppBarButton = GetTemplateChild("PART_RemoveReferenceAppBarButton") as AppBarButton;
            if (_appBarButton != null)
            {
                _appBarButton.Click += _appBarButton_Click;
            }
            if (_removeReferenceAppBarButton != null)
            {
                _removeReferenceAppBarButton.Click += _removeReferenceAppBarButton_Click;
            }
        }

        private void _removeReferenceAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var page = FindVisualParent<Page>(this);
            if (page != null && page is IDatabaseRemoveReferencePage referencePage)
            {
                referencePage.RemoveReferenceOfID(DatabaseInfo.ID);
            }
        }

        private void _appBarButton_Click(object sender, RoutedEventArgs e)
        {
            _commandBarFlyout?.ShowAt((FrameworkElement)sender);
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
