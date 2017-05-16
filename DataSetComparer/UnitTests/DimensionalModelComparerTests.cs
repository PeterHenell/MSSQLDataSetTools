using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSetTools.UnitTests
{
    [TestFixture]
    public class DimensionalModelComparerTests
    {

        [Test]
        public void ShouldCompareStar()
        {
            var a = DataTableTools.DataTableBuilder.Create("FactSales")
                .WithColumns("ID", "occoredOn", "SaleAmount", "CustomerID")
                .WithColumnTypes(typeof(int), typeof(DateTime), typeof(decimal), typeof(int))
                .WithRows(new object[]{
                    new object[] {1, DateTime.Now, 150, 1}
                }).Build();

            var b = a.Clone();

            var comparer = new DimensionalModelComparer(a, b);
            var result = comparer.Compare(a, b);
        }
    }
}
