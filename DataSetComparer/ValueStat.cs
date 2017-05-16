using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSetTools
{
    public class ValueStat
    {
        public int NonMatchingCount { get; set; }

        public List<ValueDifference> Members { get; set; }
    }
}
