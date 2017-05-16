using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSetTools.DataTableTools
{
    /// <summary>
    /// 
    /// </summary>
    public class DataTableBuilder
    {
        private string[] names;
        private Type[] types;
        private List<object[]> rows;
        private string tableName;

        private DataTableBuilder(string tableName)
        {
            this.tableName = tableName;
            this.rows = new List<object[]>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        /// <example>
        ///      var a = DataTableBuilder.Create("a")
        ///         .WithColumns("ID", "Name")
        ///         .WithRows(new object[] { 5, "Peter" })
        ///         .WithRows(new object[] { 6, "Peter" })
        ///         .Build();
        /// 
        /// </example>
        public static DataTableBuilder Create(string tableName)
        {
            return new DataTableBuilder(tableName);
        }
        public static DataTableBuilder Create()
        {
            return new DataTableBuilder("TableName");
        }


        public DataTableBuilder WithColumns(string[] names, Type[] types)
        {
            this.names = names;
            this.types = types;

            return this;
        }
        public DataTableBuilder WithColumns(params string[] names)
        {
            this.names = names;

            return this;
        }
        public DataTableBuilder WithColumnTypes(params Type[] types)
        {
            this.types = types;

            return this;
        }

        public DataTableBuilder WithRows(params object[][] rows)
        {
            foreach (var row in rows)
                this.rows.Add(row);

            return this;
        }
        public DataTableBuilder WithRows(IEnumerable<object[]> rows)
        {
            foreach (var row in rows)
                this.rows.Add(row);

            return this;
        }

        public DataTable Build()
        {
            var dt = new DataTable(tableName);
            SetColumns(dt, names, types);
            foreach (var row in this.rows)
            {
                AddRow(dt, row);
            }

            return dt;
        }

        private void AddRow(DataTable a, object[] values)
        {
            var row = a.NewRow();
            row.ItemArray = values;
            a.Rows.Add(row);
        }

        // static to make sure no private variables are manipulated or accessed.
        private static void SetColumns(DataTable a, string[] columns, Type[] types)
        {
            if (columns == null)
                return;

            if (types == null)
            {
                types = new Type[columns.Length];
                for (int i = 0; i < columns.Length; i++)
                    types[i] = typeof(object);
            }

            if (types.Length != columns.Length)
                throw new ArgumentException("Number of provided types and columns must be equal");

            for (int i = 0; i < columns.Length; i++)
            {
                var name = columns[i];
                var type = types[i];
                a.Columns.Add(name, type);
            }
        }
    }
}
