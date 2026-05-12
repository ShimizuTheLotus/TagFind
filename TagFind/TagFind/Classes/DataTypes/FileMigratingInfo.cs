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
        FileSourceNotExists,
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
