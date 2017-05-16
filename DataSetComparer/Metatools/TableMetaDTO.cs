using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSetTools.Metatools
{
    public class TableMetaDTO
    {
        public string TableSchema { get; set; }

        public string TableName { get; set; }

        public List<ColumnMetaDTO> Columns { get; set; }
    }
}
