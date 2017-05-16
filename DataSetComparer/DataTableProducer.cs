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
    public class CompareBuilder
    {
        private DataTableProducer a;
        private DataTableProducer b;
        private string outputFolder;
        private TextWriter logWriter;

        public CompareBuilder(string outputFolder, TextWriter logWriter)
        {
            this.outputFolder = outputFolder;
            this.logWriter = logWriter;
        }

        public static DataSetComparer Build(string outputFolder, TextWriter logWriter, Func<CompareBuilder, DataSetComparer> func)
        {
            return func(new CompareBuilder(outputFolder, logWriter));
        }
        public CompareBuilder To(InputTypes inputType, string query, string connectionString = "", string tableNickName = "")
        {
            this.b = DataTableProducer.Factory.Create(new CompareBuilderOptions
            {
                InputType = inputType,
                Query = query,
                ConnectionString = connectionString,
                TableNickName = tableNickName
            });
            return this;
        }
        public CompareBuilder Compare(InputTypes inputType, string query, string connectionString = "", string tableNickName = "")
        {
            a = DataTableProducer.Factory.Create(new CompareBuilderOptions
            {
                InputType = inputType,
                Query = query,
                ConnectionString = connectionString,
                TableNickName = tableNickName
            });
            return this;
        }
        public CompareBuilder Compare(DataTable dt, string resultNick)
        {
            if (dt == null)
                throw new ArgumentNullException("dt");

            a = DataTableProducer.Factory.Create(new CompareBuilderOptions
            {
                InputType = InputTypes.DataTable,
                Query = resultNick,
                DataTable = dt,
                TableNickName = resultNick,
                ConnectionString = ""
            });
            return this;
        }
        public CompareBuilder To(DataTable dt, string resultNick)
        {
            if (dt == null)
                throw new ArgumentNullException("dt");

            b = DataTableProducer.Factory.Create(new CompareBuilderOptions
            {
                InputType = InputTypes.DataTable,
                Query = resultNick,
                DataTable = dt,
                TableNickName = resultNick,
                ConnectionString = ""
            });
            return this;
        }

        public DataSetComparer End()
        {
            if (a == null)
                throw new ArgumentNullException("a");
            if (b == null)
                throw new ArgumentNullException("b");

            return new DataSetComparer(outputFolder, logWriter, a, b);
        }
    }

    public class CompareBuilderOptions
    {
        public InputTypes InputType { set; get; }
        public string Query { set; get; }
        public string ConnectionString { set; get; }
        public string TableNickName { get; set; }

        public DataTable DataTable { get; set; }
    }

    /// <summary>
    /// Class for creating lazy DataTable producers
    /// </summary>
    public class DataTableProducer
    {
        public static class Factory
        {
            public static DataTableProducer Create(CompareBuilderOptions options)
            {
                switch (options.InputType)
                {
                    case InputTypes.Query:
                        return DataTableProducer.FromQuery(options);
                    case InputTypes.CsvFile:
                        return DataTableProducer.FromCsvFile(options);
                    case InputTypes.DataTable:
                        return DataTableProducer.FromDataTable(options);
                    default:
                        throw new ArgumentException("Unimplemented argument for InputType");
                }
            }
        }

        private Action<DataTable> Callback;
        public CompareBuilderOptions Options { get; set; }

        public void Fill(DataTable dt)
        {
            if (this.Callback == null)
            {
                throw new InvalidProgramException("Producer is not initialized");
            }

            Callback(dt);
        }

        private DataTableProducer(CompareBuilderOptions options, Action<DataTable> func)
        {
            this.Options = options;
            this.Callback = func;
        }

        public static DataTableProducer FromQuery(CompareBuilderOptions options)
        {
            return new DataTableProducer(options, new Action<DataTable>(dt =>
            {
                Fill(dt, options.Query, options.ConnectionString);
            }));
        }
        public static DataTableProducer FromDataTable(CompareBuilderOptions options)
        {
            return new DataTableProducer(options, new Action<DataTable>(dt =>
            {
                foreach (DataColumn col in options.DataTable.Columns)
                {
                    dt.Columns.Add(new DataColumn(col.ColumnName));
                }
                foreach (DataRow row in options.DataTable.Rows)
                {
                    var nRow = dt.NewRow();
                    nRow.ItemArray = row.ItemArray;
                    dt.Rows.Add(nRow);
                }
                //dt = options.DataTable;
            }));
        }
        public static DataTableProducer FromCsvFile(CompareBuilderOptions options)
        {
            return new DataTableProducer(options, new Action<DataTable>(dt =>
            {
                using (var reader = File.OpenText(options.Query))
                {
                    DataTableSerializer.FromTextReader(dt, reader);
                }
            }));
        }
        public static DataTableProducer FromCsvString(CompareBuilderOptions options, TextReader reader)
        {
            return new DataTableProducer(options, new Action<DataTable>(dt =>
            {
                DataTableSerializer.FromTextReader(dt, reader);
            }));
        }

        private static DataTable Fill(DataTable dt, string sql, string connectionString)
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                using (var da = new SqlDataAdapter(sql, con))
                {
                    da.SelectCommand.CommandTimeout = int.MaxValue;
                    da.Fill(dt);
                    return dt;
                }
            }
        }
    }
}
