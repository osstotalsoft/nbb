namespace NBB.Application.DataContracts.Schema
{
    public class SchemaDefinition
    {
        public string ClassName { get; set; }
        public string Schema { get; set; }
        public string Topic { get; set; }

        public SchemaDefinition(string className, string schema, string topic)
        {
            ClassName = className;
            Schema = schema;
            Topic = topic;
        }
    }
}
