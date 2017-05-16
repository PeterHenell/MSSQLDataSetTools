using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSetTools
{
    public class ValueDifference
    {
        public string ColumnName { get; set; }
        public object ExpectedValue { get; set; }
        public object ActualValue { get; set; }
    }
}
