using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSetTools.Metatools
{
    public class ColumnMetaDTO
    {
        public string ColumnName { get; set; }

        public bool IsIdentity { get; set; }

        public bool IsNullable { get; set; }

        public int OrdinalPosition { get; set; }

        public string DataType { get; set; }
    }
}
