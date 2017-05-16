using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSetTools.Metatools
{
    public class TableMetaDataManager
    {
        private string connectionString;



        public TableMetaDataManager(string connectionString)
        {
            this.connectionString = connectionString;
        }



        public TableMetaDTO GetTableMeta(string tableSchema, string tableName)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(GET_COLUMNS_FOR_TABLE_QUERY, conn))
            {
                conn.Open();

                cmd.Parameters.Add("@objectName", System.Data.SqlDbType.VarChar).Value = string.Format("{0}.{1}", tableSchema, tableName);
                var reader = cmd.ExecuteReader();

                var columns = DataReaderTools.DataReaderToDTOMapper.DataReaderToObject<ColumnMetaDTO>(reader);

                return new TableMetaDTO
                {
                    TableSchema = tableSchema,
                    TableName = tableName,
                    Columns = columns
                };
            }
        }

        readonly string GET_COLUMNS_FOR_TABLE_QUERY = @"
            select 
                name As ColumnName, 
                CASE datatyp.datatypen
                    WHEN 'varchar' THEN datatyp.datatypen + '(' + case cols.max_length when -1 then 'max' else CAST(cols.max_length AS VARCHAR(100)) end + ')'
                    WHEN 'nvarchar' THEN datatyp.datatypen + '(' + case cols.max_length when -1 then 'max' else CAST(cols.max_length / 2 AS VARCHAR(100)) end + ')'
                    WHEN 'char' THEN datatyp.datatypen + '(' + CAST(cols.max_length AS VARCHAR(100)) + ')'
                    WHEN 'nchar' THEN datatyp.datatypen + '(' + CAST(cols.max_length / 2 AS VARCHAR(100)) + ')'
                    WHEN 'decimal' THEN datatyp.datatypen + '(' + CAST(cols.precision AS VARCHAR(100)) + ', ' + CAST(cols.scale AS VARCHAR(100)) +')'
                    WHEN 'varbinary' THEN datatyp.datatypen + '(' + case cols.max_length when -1 then 'max' else CAST(cols.max_length AS VARCHAR(100)) end + ')'
                    WHEN 'datetime2' THEN datatyp.datatypen + '(' + CAST(cols.scale AS VARCHAR(100)) + ')'
                    ELSE datatyp.datatypen
                END as DataType
                , column_id as OrdinalPosition
                , is_identity as IsIdentity 
                , is_nullable as IsNullable	
            from 
                sys.columns cols WITH(NOLOCK)
            cross apply
            (
	            select 	 	top 1
		            COALESCE(bt.name, t.name) as DataTypen
	            from
		            sys.types AS t WITH(NOLOCK)
	            LEFT OUTER JOIN 
		            sys.types AS bt WITH(NOLOCK)
		            ON t.is_user_defined = 1
		            AND bt.is_user_defined = 0
		            AND t.system_type_id = bt.system_type_id
		            AND t.user_type_id <> bt.user_type_id
		
		            where t.system_type_id = cols.system_type_id and  cols.user_type_id = t.user_type_id
            ) datatyp(datatypen)

            where object_id=object_id(@objectName)  and cols.is_computed = 0";

    }
}
