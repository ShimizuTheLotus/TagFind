using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TagFind.Classes.DB;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI
{
    public sealed partial class DatabaseListView : Control
    {
        private ListView? _listView;

        public delegate void RequestOpenDatabaseEventHandler(object sender, long ID);
        public event RequestOpenDatabaseEventHandler? RequestOpenDatabase;

        public delegate void RequestRemoveDatabaseReferenceEventHandler(object sender, long ID);
        public event RequestRemoveDatabaseReferenceEventHandler? RequestRemoveDatabaseReference;

        public static readonly DependencyProperty DatabaseListSourceProperty =
            DependencyProperty.Register(nameof(DatabaseList),
                typeof(List<MetaData>),
                typeof(DatabaseListView),
                null);

        public bool IsItemClickEnabled => _listView?.IsItemClickEnabled ?? false;

        public List<MetaData> DatabaseList
        {
            get
            {
                return (List<MetaData>)GetValue(DatabaseListSourceProperty);
            }
            set
            {
                SetValue(DatabaseListSourceProperty, value);
                if (_listView != null)
                {
                    _listView.ItemsSource = value;
                }
            }
        }

        public DatabaseListView()
        {
            DefaultStyleKey = typeof(DatabaseListView);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _listView = GetTemplateChild("PART_ListView") as ListView;

            if (_listView != null)
            {
                _listView.ItemsSource = DatabaseList;
                _listView.IsItemClickEnabled = true;
                _listView.ItemClick += _listView_ItemClick;
                _listView.SelectionMode = ListViewSelectionMode.None;
            }
        }

        private void _listView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is MetaData item)
            {
                RequestOpenDatabase?.Invoke(this, item.ID);
            }
        }
    }
}
