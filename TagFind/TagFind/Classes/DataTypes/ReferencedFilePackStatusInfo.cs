using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes.DataTypes
{
    public enum FileMigationStatus
    {
        Waiting,
        Succeeded,
        Failed
    }

    public class ReferencedFilePackStatusInfo
    {
        public ReferencedFileInfo ReferencedFileInfo { get; set; } = new();
        public FileMigationStatus FileMigationStatus { get; set; } = FileMigationStatus.Waiting;
        public InfoBarSeverity InfoBarSeverity { get; set; } = InfoBarSeverity.Warning;
        public string Exception { get; set; } = string.Empty;
    }
}
