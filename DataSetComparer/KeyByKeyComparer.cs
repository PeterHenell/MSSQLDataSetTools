using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSetTools
{
    public class KeyByKeyComparer
    {
        private DetailedCompareResult _result;
        private DataSetComparer.DataRowComparer _rowComparer = new DataSetComparer.DataRowComparer();

        public DetailedCompareResult Compare(
            System.Data.DataTable a,
            System.Data.DataTable b,
            IEnumerable<string> keys,
            IEnumerable<string> ignoredColumns)
        {
            bool isIgnoringAnyKeyColumn = ignoredColumns.Any(x => keys.Any(y => y == x));
            if (isIgnoringAnyKeyColumn)
                throw new ArgumentException("ignoredColumns cannot contain columns which are part of the key");

            RemoveIgnoredColumns(a, b, ignoredColumns);
            ValidateResultColumns(a, b);

            _result = new DetailedCompareResult
            {
                LeftRowCount = a.Rows.Count,
                RightRowCount = b.Rows.Count,
            };

            DataSet set = new DataSet();
            set.Tables.Add(a);
            set.Tables.Add(b);

            var aKeyColumns = GetKeyColumns(a, keys);
            var bKeyColumns = GetKeyColumns(b, keys);

            var relation = new DataRelation("key", aKeyColumns, bKeyColumns, false);
            set.Relations.Add(relation);

            InternalCompare(a, aKeyColumns, relation);

            return _result;
        }

        private void ValidateResultColumns(DataTable a, DataTable b)
        {
            var aCols = a.Columns.ToList().Where(ac => !b.Columns.Contains(ac.ColumnName)).Select(c => c.ColumnName).ToList();
            var bCols = b.Columns.ToList().Where(bc => !a.Columns.Contains(bc.ColumnName)).Select(c => c.ColumnName).ToList();

            if (aCols.Count > 0 || bCols.Count > 0)
            {
                throw new ArgumentException("Column names in both result sets must match");
            }

            if (a.Columns.Count == 0 || b.Columns.Count == 0)
            {
                throw new ArgumentException("Must have at least one column to compare");
            }
        }

        private void InternalCompare(System.Data.DataTable parentDataTable, DataColumn[] aKeyColumns, DataRelation relation)
        {
            Parallel.ForEach(parentDataTable.AsEnumerable(), (DataRow row) =>
            {
                if (_result.ComparisonErrors.Count > 10000)
                    return;

                var matchedRows = row.GetChildRows(relation); // matches based on the relation
                var keyValues = GetKeyValuesOnRow(row, aKeyColumns);

                if (matchedRows.Length > 0)
                {
                    // match was found
                    CompareValuesForMatchedRows(row, matchedRows, keyValues);
                    CompareNumberOfMatches(matchedRows, keyValues);
                }
                else
                {
                    // could not find key in b
                    MarkRowAsMissing(keyValues);
                }
            });
        }

        private object[] GetKeyValuesOnRow(DataRow row, DataColumn[] keyColumns)
        {
            var keyValues = keyColumns.Select(k => row[k]).ToArray();
            return keyValues;
        }

        private static DataColumn[] GetKeyColumns(System.Data.DataTable dt, IEnumerable<string> keys)
        {
            var aKeyColumns = dt.Columns.ToList().Where(c => keys.Contains(c.ColumnName)).ToArray();
            return aKeyColumns;
        }

        private static void RemoveIgnoredColumns(System.Data.DataTable a, System.Data.DataTable b, IEnumerable<string> ignoredColumns)
        {
            if (ignoredColumns == null)
                ignoredColumns = new List<string>();

            foreach (var ignoredColumn in ignoredColumns)
            {
                a.Columns.Remove(ignoredColumn);
                b.Columns.Remove(ignoredColumn);
            }
        }

        public DetailedCompareResult Compare(System.Data.DataTable a, System.Data.DataTable b, IEnumerable<string> keys)
        {
            return Compare(a, b, keys, ignoredColumns: new List<string>());
        }

        private static object[] GetKeyValuesOnIndex(System.Data.DataTable dt, DataColumn[] keyColumns, int i)
        {
            var keyValues = keyColumns.Select(k => dt.Rows[i][k]).ToArray();
            return keyValues;
        }

        private void MarkRowAsMissing(object[] keyValues)
        {
            lock (_lock)
            {
                _result.ComparisonErrors.Add(
                    new CompareError
                    {
                        KeyValue = keyValues,
                        MissingInB = true
                    });
            }
        }

        private void CompareNumberOfMatches(DataRow[] matchedRows, object[] keyValues)
        {
            if (matchedRows.Length > 1)
            {
                // multiple rows found on this key
                lock (_lock)
                {
                    _result.ComparisonErrors.Add(
                        new CompareError
                        {
                            KeyValue = keyValues,
                            //RowIndex = i,
                            MultipleRowPerKeyInB = true
                        });
                }
            }
        }

        private object _lock = new object();

        private void CompareValuesForMatchedRows(DataRow row, DataRow[] matchedRows, object[] keyValues)
        {
            Parallel.ForEach(matchedRows, (matchedRow) =>
            {

                if (!_rowComparer.Equals(row, matchedRow))
                {
                    List<ValueDifference> diffs = _rowComparer.GetDiffs(row, matchedRow);
                    // row values does not match
                    lock (_lock)
                    {
                        _result.ComparisonErrors.Add(new CompareError
                        {
                            KeyValue = keyValues,
                            //RowIndex = i,
                            ValueDifferences = diffs,
                            ValuesDoesNotMatch = true
                        });
                    }
                }
            });
        }
    }
}
