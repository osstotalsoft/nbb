using FluentAssertions;
using NBB.Exporter.Csv;
using System.Collections.Generic;
using System.IO;
using Xunit;


namespace Nbb.Exporter.Csv.Tests
{
    public class CsvExporterTests
    {
        private readonly List<Customer> _list;
        public CsvExporterTests()
        {
            _list = GetList();
        }

        private List<Customer> GetList()
        {
            var numberOfElements = 10;
            var list = new List<Customer>();
            for (var i = 0; i < numberOfElements; i++)
                list.Add(new Customer
                {
                    CustomerId = i,
                    Name = "Customer " + i
                });
            return list;
        }

        [Fact]
        public void Should_Generate_csv()
        {
            //Arrange
            var list = _list;

            var exporter = new CsvDataExport<Customer>();
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "excel1.csv");


            //Act
            var stream = exporter.Export(list, new Dictionary<string, string>(), null);
            using (var fileStream = File.Create(filePath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }

            var fileExistsAndNotEmpty = File.Exists(filePath) && new FileInfo(filePath).Length > 0;

            if (File.Exists(filePath))
                File.Delete(filePath);

            //Assert
            stream.Should().NotBeNull();
            stream.Length.Should().BeGreaterThan(0);
            fileExistsAndNotEmpty.Should().BeTrue();
        }
    }

    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
    }
}
