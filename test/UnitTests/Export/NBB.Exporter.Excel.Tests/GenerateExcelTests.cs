using FluentAssertions;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace NBB.Exporter.Excel.Tests
{
    public class GenerateExcelTests
    {
        private List<Customer> _list;
        public GenerateExcelTests()
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
        public void Should_Generate_excel()
        {
            //Arrange
            var list = _list;

            var exporter = new ExcelDataExport<Customer>();
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "excel1.xlsx");
            

            //Act
            var stream = exporter.Export(list, new Dictionary<string, string>(), null);
            using (var fileStream = File.Create(filePath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }

            var fileExistsAndNotEmpty = File.Exists(filePath) && new FileInfo(filePath).Length > 0;

            if(File.Exists(filePath))
                File.Delete(filePath);

            //Assert
            stream.Should().NotBeNull();
            stream.Length.Should().BeGreaterThan(0);
            fileExistsAndNotEmpty.Should().BeTrue();
        }

        [Fact]
        public void Should_Generate_excel_with_custom_headers()
        {
            //Arrange
            var list = _list;
            var headers = new List<string>
            {
                "ID",
                "Nume"
            };
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "excel2.xlsx");
            var exporter = new ExcelDataExport<Customer>();

            //Act
            
            var stream = exporter.Export(list, new Dictionary<string, string>(), headers);
            using (var fileStream = File.Create(filePath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }

            var fileExistsAndNotEmpty = File.Exists(filePath) && new FileInfo(filePath).Length > 0;
            
            if(File.Exists(filePath))
                File.Delete(filePath);

            //Assert

            stream.Should().NotBeNull();
            stream.Length.Should().BeGreaterThan(0);
            fileExistsAndNotEmpty.Should().BeTrue();
        }

    }
}