using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DataSetTools
{
    public class CompareResult
    {
        private Dictionary<string, DataTable> _dict;
        private object _lock = new object();
        public CompareResult()
        {
            _dict = new Dictionary<string, DataTable>();
        }

        public void Add(System.Data.DataTable result, string outputFile)
        {
            lock (_lock)
            {
                _dict.Add(outputFile, result);
            }
        }

        

        public void FormatResult(StringBuilder sb)
        {
            lock (_lock)
            {
                foreach (var kv in _dict)
                {
                    sb.AppendLine(kv.Key + " :");
                    var name = kv.Key;
                    var result = kv.Value;
                    
                    foreach (DataRow row in result.Rows)
                    {
                        sb.AppendLine(string.Join("|", row.ItemArray));
                    }
                }
            }
        }
    }
}
