using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSetTools
{
    public class DetailedStats
    {
        public DetailedStats()
        {
            Values = new Dictionary<string, ValueStat>();
        }

        public Dictionary<string, ValueStat> Values { get; private set; }
    }
}
