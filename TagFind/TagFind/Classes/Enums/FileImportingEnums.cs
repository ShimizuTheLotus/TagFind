using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes.Enums
{
    public enum ImportModeEnum
    {
        //ImportAllFiles,
        ImportAllReferencedFiles,
        ImportAbsentFilesOnly
    }
    public enum FileImportOptionEnum
    {
        OriginalPath,
        MigratePath
    }
    public enum ConflictPreferenceEnum
    {
        Skip,
        Replace,
        UserDecide
    }
}
