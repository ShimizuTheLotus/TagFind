using Microsoft.Data.Sqlite;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes.ML
{
    public class TagML
    {
        private MLContext MLContext = new();
        private ITransformer Model;

        public class DocumentData
        {
            public string Content { get; set; } = string.Empty;
            public List<long> TagID { get; set; } = [];
        }

    }
}
