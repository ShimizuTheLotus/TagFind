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
    public sealed partial class DatabaseListViewItem : Control
    {
        public delegate void RequestOpenDatabaseEventHandler(object sender, long ID);
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
    }
}
