using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSetTools
{
    public class CompareError
    {
        public bool MissingInB { get; set; }
        public bool MultipleRowPerKeyInB { get; set; }
        public object[] KeyValue { get; set; }

        public int RowIndex { get; set; }

        public bool ValuesDoesNotMatch { get; set; }
        public List<ValueDifference> ValueDifferences { get; set; }
    }
}
