File exporter
===============

Allows export of a generic list to CVS (comma separated value).
The separator can be configured using the key "Separator" in the properties dictionary (see sample).
Headers can be specified or will be automatically detected and set to the C# property names of the class T.

Simple usage
----------------

```csharp
using NBB.Exporter;
using NBB.Exporter.Csv;

public class CsvExportExample
{
	private readonly IAbstractDataExport<Customer> _exporter;
	
	public CsvExportExample(IAbstractDataExport<Customer> exporter)
	{
		_exporter = exporter;
	}

	void ExportToCurrentDirectory(List<Customer> list, string fileName,)
	{
		var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
		var properties = new Dictionary<string, string>();
		properties["Separator"]=",";
		var stream = exporter.Export(list, properties, null);
		using (var fileStream = File.Create(filePath))
		{
			stream.Seek(0, SeekOrigin.Begin);
			stream.CopyTo(fileStream);
		}
	}
}
```