using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSetTools
{
    public class DataTableSerializer
    {
        public const char COLSEPARATOR = ',';
        public const char VALUESEPARATOR = '|';

        public static void FromTextReader(DataTable dt, TextReader reader)
        {
            //good ol absolutely no error catching at all, expect to win, never fail.
            // In reality, first row should be column names comma separated and the rest should be value rows pipe separated

            var cols = reader.ReadLine().Split(COLSEPARATOR);
            foreach (var col in cols)
            {
                dt.Columns.Add(col);
            }

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                dt.Rows.Add(line.Split(VALUESEPARATOR));
            }
        }

        public static void ToFile(DataTable dt, TextWriter writer)
        {
            var aCols = GetColumnsAsString(dt);

            writer.WriteLine(aCols);
            foreach (var row in dt.AsEnumerable())
            {
                var s = string.Join(VALUESEPARATOR.ToString(), row.ItemArray);
                writer.WriteLine(s);
            }
            writer.Flush();
        }

        public static string GetColumnsAsString(DataTable a)
        {
            var aCols = string.Join(COLSEPARATOR.ToString(), a.Columns.ToList()).ToLowerInvariant();
            return aCols;
        }
    }
}
