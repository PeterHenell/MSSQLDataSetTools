using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSetTools
{
    public class DataSetComparer
    {
        private string outputFolder;
        private TextWriter logWriter;
        private DataTableProducer queryA;
        private DataTableProducer queryB;


        public DataSetComparer(string outputFolder, TextWriter logWriter, DataTableProducer qa, DataTableProducer qb)
        {
            if (qa == null)
                throw new ArgumentNullException("queryA");
            if (qb == null)
                throw new ArgumentNullException("queryB");
            if (qa.Options == null)
                throw new ArgumentNullException("queryA.Options");
            if (qb.Options == null)
                throw new ArgumentNullException("queryB.Options");

            this.outputFolder = outputFolder;
            this.logWriter = logWriter;
            this.queryA = qa;
            this.queryB = qb;

            if (qa.Options.TableNickName == qb.Options.TableNickName)
            {
                throw new ArgumentException("output file names cannot be the same for both queries.");
            }
        }


        public CompareResult CompareCommands()
        {
            var result = new CompareResult();
            var runA = new { Table = new DataTable(queryA.Options.TableNickName), Producer = queryA };
            var runB = new { Table = new DataTable(queryB.Options.TableNickName), Producer = queryB };
            var list = new[] { runA, runB };

            logWriter.WriteLine("Running queries");
            Parallel.ForEach(list, e =>
            {
                e.Producer.Fill(e.Table);
            });

            logWriter.WriteLine("Creating diff of results");

            var op = new ParallelOptions();
            Parallel.ForEach(list, op, (x, y, i) =>
            {
                var a = list[i];
                var b = list[i ^ 1];
                logWriter.WriteLine("Comparing {0} vs {1}", a, b);
                var resultKey = string.Format("{0} vs {1}", a.Table.TableName, b.Table.TableName);
                var r = CompareTables(a.Table, b.Table, resultKey);
                result.Add(r, resultKey);
            });
            return result;
        }

        public List<string> PeekResultSetColumnNames()
        {
            var runA = new { Table = new DataTable(queryA.Options.TableNickName), Producer = queryA };
            runA.Producer.Fill(runA.Table);
            return runA.Table.Columns.ToList().Select(c => c.ColumnName).ToList();
        }

        public DetailedCompareResult GetDetailedDifferences(
            IEnumerable<string> keyColumnNames, 
            IEnumerable<string> ignoredColumns)
        {
            var runA = new { Table = new DataTable(queryA.Options.TableNickName), Producer = queryA };
            var runB = new { Table = new DataTable(queryB.Options.TableNickName), Producer = queryB };
            var list = new[] { runA, runB };

            logWriter.WriteLine("Running queries");
            Parallel.ForEach(list, e =>
            {
                e.Producer.Fill(e.Table);
            });
            var keyByKeyComparer = new KeyByKeyComparer();
            return keyByKeyComparer.Compare(runA.Table, runB.Table, keyColumnNames, ignoredColumns);
        }

        public DetailedCompareResult GetDetailedDifferences(IEnumerable<string> keyColumnNames)
        {
            return GetDetailedDifferences(keyColumnNames, new List<string>());
        }

        private DataTable CompareTables(DataTable a, DataTable b, string outputFileName)
        {
            var differentRows = a.AsEnumerable().Except(b.AsEnumerable(), DataRowComparer.Default);
            var diffTable = a.Clone();
            foreach (var row in differentRows)
            {
                diffTable.Rows.Add(row.ItemArray);
            }

            logWriter.WriteLine("Identified {0} different rows comparing {1} to {2}", diffTable.Rows.Count, a.TableName, b.TableName);

            var aCols = DataTableSerializer.GetColumnsAsString(a);
            var bCols = DataTableSerializer.GetColumnsAsString(b);

            if (aCols != bCols)
            {
                logWriter.WriteLine(aCols);
                logWriter.WriteLine(bCols);
                logWriter.WriteLine("FATAL, column lists do not match");
            }
            return diffTable;
        }

        public void SaveTable(DataTable a, string fileName)
        {
            using (var writer = File.CreateText(fileName))
            {
                DataTableSerializer.ToFile(a, writer);
            }
        }

        public class DataRowComparer : IEqualityComparer<DataRow>
        {
            static DataRowComparer _comparer;
            public static DataRowComparer Default
            {
                get
                {
                    if (_comparer == null)
                        _comparer = new DataRowComparer();
                    return _comparer;
                }
            }

            public bool Equals(DataRow x, DataRow y)
            {
                var a = string.Join(",", x.ItemArray).ToLowerInvariant();
                var b = string.Join(",", y.ItemArray).ToLowerInvariant();
                return a == b;
            }

            public int GetHashCode(DataRow obj)
            {
                var a = string.Join(",", obj.ItemArray).ToLowerInvariant();
                return a.GetHashCode();
            }

            internal List<ValueDifference> GetDiffs(DataRow expectedRow, DataRow actualRow)
            {
                var diffs = new List<ValueDifference>();
                for (int i = 0; i < expectedRow.ItemArray.Length; i++)
                {
                    if (!expectedRow[i].Equals(actualRow[i]))
                    {
                        var diff = new ValueDifference
                        {
                            ColumnName = expectedRow.Table.Columns[i].ColumnName,
                            ExpectedValue = expectedRow[i],
                            ActualValue = actualRow[i]
                        };
                        diffs.Add(diff);
                    }
                }
                return diffs;
            }
        }
    }

    public static class DataColumnExtensions
    {
        public static List<DataColumn> ToList(this DataColumnCollection columns)
        {
            var list = new List<DataColumn>();
            foreach (DataColumn col in columns)
            {
                list.Add(col);
            }
            return list;
        }
    }
}
