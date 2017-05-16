using DataSetTools.DataReaderTools;
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
    public class DataTableToObjectMapperTests
    {
        public struct MockStruct
        {
            public int Age;
            public string Name;
        }
        public class MockObject
        {
            public int Age { get; set; }
            public string Name { get; set; }
        }

        [Test]
        public void ShouldMapReaderToStruct()
        {
            var dt = DataTableBuilder.Create()
                .WithColumns("Age", "Name")
                .WithRows(new object[] { 5, "Peter" })
                .Build();

            var reader = dt.CreateDataReader();
            var mappedObjects = DataReaderToDTOMapper.DataReaderToStruct<MockStruct>(reader);

            Assert.That(mappedObjects.Count, Is.EqualTo(1));
            var o = mappedObjects.FirstOrDefault();

            Assert.That(o.Age, Is.EqualTo(5));
            Assert.That(o.Name, Is.EqualTo("Peter"));
        }

        [Test]
        public void ShouldMapReaderToObject()
        {
            var dt = DataTableBuilder.Create()
                 .WithColumns("Age", "Name")
                 .WithRows(new object[] { 5, "Peter" })
                 .Build();

            var reader = dt.CreateDataReader();
            var mappedObjects = DataReaderToDTOMapper.DataReaderToObject<MockObject>(reader);

            Assert.That(mappedObjects.Count, Is.EqualTo(1));
            var o = mappedObjects.FirstOrDefault();

            Assert.That(o.Age, Is.EqualTo(5));
            Assert.That(o.Name, Is.EqualTo("Peter"));
        }

        [Test]
        public void ShouldNotAllowDifferentAmountOfColumnsInDataReaderAndObject()
        {
            var dt = DataTableBuilder.Create()
                .WithColumns("Age") // Name is missing in the DataTable
                .WithRows(new object[] { 5 })
                .Build();

            var reader = dt.CreateDataReader();
            // check for object
            Assert.Throws<ArgumentException>(() => DataReaderToDTOMapper.DataReaderToObject<MockObject>(reader));
            // check for struct
            Assert.Throws<ArgumentException>(() => DataReaderToDTOMapper.DataReaderToStruct<MockStruct>(reader));
        }
    }
}
