namespace NBB.Application.DataContracts.Schema
{
    public class SchemaDefinition
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Schema { get; set; }
        public string Topic { get; set; }
        public string SampleJson { get; set; }

        public SchemaDefinition(string name, string fullName, string schema, string topic, string sampleJson)
        {
            Name = name;
            FullName = fullName;
            Schema = schema;
            Topic = topic;
            SampleJson = sampleJson;
        }
    }
}