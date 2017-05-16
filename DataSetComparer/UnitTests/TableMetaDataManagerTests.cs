using DataSetTools.Metatools;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSetTools.UnitTests
{
    [TestFixture]
    public class TableMetaDataManagerTests
    {

        [Test]
        public void ShouldGetMetaForTable()
        {
            TableMetaDataManager manager = new TableMetaDataManager(GetConnectionString());
            var meta = manager.GetTableMeta("dbo", "DATABASECHANGELOG");

            Assert.That(meta.TableName, Is.EqualTo("DATABASECHANGELOG"));
            Assert.That(meta.TableSchema, Is.EqualTo("dbo"));
            Assert.That(meta.Columns.Count, Is.GreaterThan(0));

            var dateExecuted = meta.Columns.FirstOrDefault(c => c.ColumnName == "DATEEXECUTED");
            Assert.That(dateExecuted.ColumnName, Is.EqualTo("DATEEXECUTED"));
            Assert.That(dateExecuted.IsIdentity, Is.EqualTo(false));
            Assert.That(dateExecuted.IsNullable, Is.EqualTo(false));
            Assert.That(dateExecuted.OrdinalPosition, Is.EqualTo(4));
            StringAssert.AreEqualIgnoringCase(dateExecuted.DataType, "DATETIME2(3)");

            var md5sum = meta.Columns.FirstOrDefault(c => c.ColumnName == "MD5SUM");
            Assert.That(md5sum.IsNullable, Is.EqualTo(true));
        }

        private string GetConnectionString()
        {
            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            builder.DataSource = @"localhost\peheintegration";
            builder.InitialCatalog = "ConfigurationManagement";
            builder.IntegratedSecurity = true;
            return builder.ToString();
        }

    }
}

