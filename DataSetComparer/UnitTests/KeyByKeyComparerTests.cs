using DataSetTools.DataTableTools;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSetTools.UnitTests
{
    [TestFixture]
    public class KeyByKeyComparerTests
    {

        KeyByKeyComparer comparer = new KeyByKeyComparer();

        [Test]
        public void ShouldCompareRowsByKeys()
        {
            var a = DataTableBuilder.Create("a")
                     .WithColumns("ID", "Name")
                     .WithRows(new object[] { 5, "Peter" })
                     .Build();
            var b = DataTableBuilder.Create("b")
                     .WithColumns("ID", "Name")
                     .WithRows(new object[] { 5, "Peter" })
                     .Build();

            var keys = new List<string>();
            keys.Add("ID");

            var result = comparer.Compare(a, b, keys);

            Assert.That(result.ComparisonErrors.Count, Is.EqualTo(0));
        }

        [Test]
        public void ShouldIdentifyMissingKey()
        {
            var a = DataTableBuilder.Create("a")
                     .WithColumns("ID", "Name")
                     .WithRows(new object[] { 5, "Peter" })
                     .Build();
            var b = DataTableBuilder.Create("b")
                     .WithColumns("ID", "Name")
                     .WithRows(new object[] { 4, "Peter" })
                     .Build();

            var keys = new List<string>();
            keys.Add("ID");

            var result = comparer.Compare(a, b, keys);


            Assert.That(result.ComparisonErrors.Where(e => e.MissingInB).Count(), Is.EqualTo(1));
            // Even if the values "Peter" and "Henell" does not match they should not end up as ValuesDoesNotMatch
            // because the key did not match
            Assert.That(result.ComparisonErrors.Where(e => e.ValuesDoesNotMatch).Count(), Is.EqualTo(0));
        }



        [Test]
        public void ShouldFindUnmatchingValuesPerKey()
        {
            var a = DataTableBuilder.Create("a")
                     .WithColumns("ID", "Name")
                     .WithRows(new object[] { 5, "Peter" })
                     .Build();
            var b = DataTableBuilder.Create("b")
                     .WithColumns("ID", "Name")
                     .WithRows(new object[] { 5, "Henell" })
                     .Build();

            var keys = new List<string>();
            keys.Add("ID");


            var result = comparer.Compare(a, b, keys);

            Assert.That(result.ComparisonErrors.Where(e => e.ValuesDoesNotMatch).Count(), Is.EqualTo(1));
            var diffValue = result.ComparisonErrors.Where(e => e.ValuesDoesNotMatch).FirstOrDefault().ValueDifferences.FirstOrDefault();
            Assert.That(diffValue.ColumnName, Is.EqualTo("Name"));
            Assert.That(diffValue.ExpectedValue, Is.EqualTo("Peter"));
            Assert.That(diffValue.ActualValue, Is.EqualTo("Henell"));
        }

        [Test]
        public void ShouldAllowDuplicateKeyInLeftTable()
        {
            var a = DataTableBuilder.Create("a")
                     .WithColumns("ID", "Name")
                     .WithRows(new object[] { 5, "Peter" })
                     .WithRows(new object[] { 5, "Peter" })
                     .Build();
            var b = DataTableBuilder.Create("b")
                     .WithColumns("ID", "Name")
                     .WithRows(new object[] { 5, "Peter" })
                     .Build();

            var keys = new List<string>();
            keys.Add("ID");


            var result = comparer.Compare(a, b, keys);

            Assert.That(result.ComparisonErrors.Where(e => e.MultipleRowPerKeyInB).Count(), Is.EqualTo(0));
            Assert.That(result.ComparisonErrors.Where(e => e.MissingInB).Count(), Is.EqualTo(0));
            Assert.That(result.ComparisonErrors.Where(e => e.ValuesDoesNotMatch).Count(), Is.EqualTo(0));
        }

        [Test]
        public void ShouldNotAllowDuplicatesInRightTable()
        {
            var a = DataTableBuilder.Create("a")
                     .WithColumns("ID", "Name")
                     .WithRows(new object[] { 5, "Peter" })
                     .Build();
            var b = DataTableBuilder.Create("b")
                     .WithColumns("ID", "Name")
                     .WithRows(new object[] { 5, "Henell" })
                     .WithRows(new object[] { 5, "Henell" })
                     .Build();

            var keys = new List<string>();
            keys.Add("ID");


            var result = comparer.Compare(a, b, keys);

            Assert.That(result.ComparisonErrors.Where(e => e.MultipleRowPerKeyInB).Count(), Is.EqualTo(1));
        }

        [Test]
        public void ShouldHandleNullValuesInActualRow()
        {
            var a = DataTableBuilder.Create("a")
                     .WithColumns("ID", "Name")
                     .WithRows(new object[] { 5, "Peter" })
                     .Build();
            var b = DataTableBuilder.Create("b")
                     .WithColumns("ID", "Name")
                     .WithRows(new object[] { 5, null })
                     .Build();

            var keys = new List<string>();
            keys.Add("ID");


            var result = comparer.Compare(a, b, keys);

            var diffValue = result.ComparisonErrors.Where(e => e.ValuesDoesNotMatch).FirstOrDefault().ValueDifferences.FirstOrDefault();
            Assert.That(diffValue.ColumnName, Is.EqualTo("Name"));
            Assert.That(diffValue.ExpectedValue, Is.EqualTo("Peter"));
            Assert.That(diffValue.ActualValue, Is.EqualTo(System.DBNull.Value));
        }

        [Test]
        public void ShouldHandleNullValuesInExpectedRow()
        {
            var a = DataTableBuilder.Create("a")
                     .WithColumns("ID", "Name")
                     .WithRows(new object[] { 5, null })
                     .Build();
            var b = DataTableBuilder.Create("b")
                     .WithColumns("ID", "Name")
                     .WithRows(new object[] { 5, "Peter" })
                     .Build();

            var keys = new List<string>();
            keys.Add("ID");


            var result = comparer.Compare(a, b, keys);

            var diffValue = result.ComparisonErrors.Where(e => e.ValuesDoesNotMatch).FirstOrDefault().ValueDifferences.FirstOrDefault();
            Assert.That(diffValue.ColumnName, Is.EqualTo("Name"));
            Assert.That(diffValue.ExpectedValue, Is.EqualTo(System.DBNull.Value));
            Assert.That(diffValue.ActualValue, Is.EqualTo("Peter"));
        }

        [Test]
        public void ShouldHandleNullValuesInKey()
        {
            var a = DataTableBuilder.Create("a")
                     .WithColumns("ID", "Name")
                     .WithRows(new object[] { null, "Peter" })
                     .Build();
            var b = DataTableBuilder.Create("b")
                     .WithColumns("ID", "Name")
                     .WithRows(new object[] { 5, "Henell" })
                     .Build();

            var keys = new List<string>();
            keys.Add("ID");


            var result = comparer.Compare(a, b, keys);

            Assert.That(
                    result.ComparisonErrors.Where(e => e.MissingInB).Count(),
                    Is.EqualTo(1));
        }

        [Test]
        public void ShouldIgnoreColumns()
        {
            var a = DataTableBuilder.Create("a")
                     .WithColumns("ID", "Name", "ValidFrom")
                     .WithRows(new object[] { 5, "Peter", "April" })
                     .Build();
            var b = DataTableBuilder.Create("b")
                     .WithColumns("ID", "Name", "ValidFrom")
                     .WithRows(new object[] { 5, "Peter", "June" })
                     .Build();

            var keys = new List<string> { "ID" };
            var ignored = new List<string> { "ValidFrom" };

            var result = comparer.Compare(a, b, keys, ignored);

            Assert.That(
                    result.ComparisonErrors.Count, Is.EqualTo(0));
        }

        [Test]
        public void ShouldCalculateComparisonStatistics()
        {
            var a = DataTableBuilder.Create("a")
                     .WithColumns("ID", "Name", "ValidFrom")
                     .WithRows(new object[] { 5, "Peter", "April" })
                     .Build();
            var b = DataTableBuilder.Create("b")
                     .WithColumns("ID", "Name", "ValidFrom")
                     .WithRows(new object[] { 5, "Peter", "June" })
                     .Build();

            var keys = new List<string> { "ID" };


            var result = comparer.Compare(a, b, keys);

            Assert.That(result.GetCalculatedStats().Values["ValidFrom"].NonMatchingCount, Is.EqualTo(1));
        }

        [Test]
        public void ShouldNotAllowIgnoringKeyColumns()
        {
            var a = DataTableBuilder.Create("a")
                     .WithColumns("ID", "Name", "ValidFrom")
                     .WithRows(new object[] { 5, "Peter", "April" })
                     .Build();
            var b = DataTableBuilder.Create("b")
                     .WithColumns("ID", "Name", "ValidFrom")
                     .WithRows(new object[] { 5, "Peter", "June" })
                     .Build();

            var keys = new List<string> { "ID" };
            var ignores = new List<string> { "ID" };

            Assert.Throws(typeof(ArgumentException), () => comparer.Compare(a, b, keys, ignores));
        }

        [Test]
        public void ResultColumnsDoNotMatch()
        {
            var a = DataTableBuilder.Create("a")
                     .WithColumns("aID", "aName", "aValidFrom")
                     .WithRows(new object[] { 5, "Peter", "April" })
                     .Build();
            var b = DataTableBuilder.Create("b")
                     .WithColumns("bID", "bName", "bValidFrom")
                     .WithRows(new object[] { 5, "Peter", "June" })
                     .Build();

            var keys = new List<string> { "aID" };


            Assert.Throws(typeof(ArgumentException), () => comparer.Compare(a, b, keys));
        }

        [Test]
        public void NoResultColumns()
        {
            var a = DataTableBuilder.Create("a")
                     .Build();
            var b = DataTableBuilder.Create("b")
                     .Build();

            var keys = new List<string> { "aID" };


            Assert.Throws(typeof(ArgumentException), () => comparer.Compare(a, b, keys));
        }

        [Test]
        public void EmptyResultSetsIsOK()
        {
            var a = DataTableBuilder.Create("a")
                     .WithColumns("ID", "Name", "ValidFrom")
                     .Build();
            var b = DataTableBuilder.Create("b")
                     .WithColumns("ID", "Name", "ValidFrom")
                     .Build();

            var keys = new List<string> { "ID" };

            comparer.Compare(a, b, keys);
        }
    }
}
