File exporter
===============

Allows export of a generic list to excel.
Uses NPOI as current implementation in NBB.Exporter.Excel to export a List<T> to excel, but more exporters can be added.
The sheet name can be configured in the properties dictionary as "SheetName" (see sample). 
Headers can be specified or will be automatically detected and set to the C# property names of the class T.

Simple usage:
----------------

```csharp
using NBB.Exporter;
using NBB.Exporter.Excel;

public class ExcelExportExample
{
	private readonly IAbstractDataExport<Customer> _exporter;	
	
	public ExcelExportExample(IAbstractDataExport<Customer> exporter)
	{
		_exporter = exporter;
	}

	void ExportToCurrentDirectory(List<Customer> list, string fileName,)
	{
		var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
		var properties = new Dictionary<string, string>();
		properties["SheetName"]="CustomerData";

		var stream = exporter.Export(list, properties, null);
		using (var fileStream = File.Create(filePath))
		{
			stream.Seek(0, SeekOrigin.Begin);
			stream.CopyTo(fileStream);
		}
	}
}
```