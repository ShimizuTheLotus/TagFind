using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes.DataTypes
{
    public enum FileNavigationStatus
    {
        Waiting,
        Succeeded,
        Failed
    }

    public class ReferencedFilePackStatusInfo
    {
        public ReferencedFileInfo ReferencedFileInfo { get; set; } = new();
        public FileNavigationStatus FileNavigationStatus { get; set; } = FileNavigationStatus.Waiting;
        public string Exception { get; set; } = string.Empty;
    }
}
