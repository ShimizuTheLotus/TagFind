using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes.DataTypes
{
    public enum FileMigratingStatusEnum
    {
        Migrating,
        Succeeded,
        FileSourceNotExists,// File source is a zip archive or a folder, containing the files to be migrated.
        FileSourceNotValid,
        SourceFileNotExists,// Source file is the file to be migrated, which is inside the file source.
        Conflict,
        OperationFailed,
        FailedForUnknownReason
    }

    public class FileMigratingInfo
    {
        public string Path = "";
        public FileMigratingStatusEnum FileMigratingStatus = FileMigratingStatusEnum.Migrating;
    }
}
