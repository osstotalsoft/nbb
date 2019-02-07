using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;

namespace NBB.EventStore.AdoNet.Internal
{
    public class Scripts
    {

        private readonly ConcurrentDictionary<string, string> _scripts
            = new ConcurrentDictionary<string, string>();

        public string AppendEventsToStreamExpectedVersion => GetScript(nameof(AppendEventsToStreamExpectedVersion));
        public string AppendEventsToStreamExpectedVersionAny => GetScript(nameof(AppendEventsToStreamExpectedVersionAny));
        public string CreateDatabaseObjects => GetScript(nameof(CreateDatabaseObjects));
        public string DropDatabaseObjects => GetScript(nameof(DropDatabaseObjects));
        public string GetEventsFromStream => GetScript(nameof(GetEventsFromStream));
        public string GetSnapshotForStream => GetScript(nameof(GetSnapshotForStream));
        public string SetSnapshotForStream => GetScript(nameof(SetSnapshotForStream));

        private string GetScript(string name)
        {
            return _scripts.GetOrAdd(name,
                key =>
                {
                    using (Stream stream = typeof(Scripts).GetTypeInfo().Assembly.GetManifestResourceStream("NBB.EventStore.AdoNet.Internal.SqlScripts." + key + ".sql"))
                    {
                        if (stream == null)
                        {
                            throw new Exception($"Embedded resource, {name}, not found.");
                        }
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            return reader
                                .ReadToEnd();
                        }
                    }
                });
        }
    }
}
