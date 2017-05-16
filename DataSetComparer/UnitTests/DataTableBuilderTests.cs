using DataSetTools.DataTableTools;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSetTools.UnitTests
{
    [TestFixture]
    public class DataTableBuilderTests
    {
        [Test]
        public void ShouldBuildADataTableWithOneRow()
        {
            var dt = DataTableBuilder.Create()
                .WithColumns("ID", "Name")
                .WithRows(new object[] { 1, "Peter" })
                .Build();

            Assert.That(dt.Rows.Count, Is.EqualTo(1));

            var firstRow = dt.Rows[0];
            Assert.That(firstRow["Name"], Is.EqualTo("Peter"));
            Assert.That(firstRow["ID"], Is.EqualTo(1));
        }

        [Test]
        public void ShouldBuildADataTableWithSeveralRows()
        {
            var dt = DataTableBuilder.Create()
                .WithColumns("ID", "Name")
                .WithRows(new object[] { 1, "Peter" })
                .WithRows(new object[] { 2, "Henell" })
                .WithRows(new object[] { 3, "Anders" })
                .WithRows(new object[] { 4, "Johan" })
                .Build();

            Assert.That(dt.Rows.Count, Is.EqualTo(4));
        }

        [Test]
        public void ShouldBuildEmptyDataTable()
        {
            var dt = DataTableBuilder.Create()
                .Build();

            Assert.That(dt.Rows.Count, Is.EqualTo(0));
        }
    }
}
