using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSetTools
{
    public class DimensionalModelComparer
    {
        private DataTable b;
        private DataTable a;

        public DimensionalModelComparer(DataTable a, DataTable b)
        {
            this.a = a;
            this.b = b;
        }

        public DetailedCompareResult Compare(DataTable a, DataTable b)
        {
            return null;
        }
    }
}
