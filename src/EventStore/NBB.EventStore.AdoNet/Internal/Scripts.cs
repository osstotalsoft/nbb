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
        private readonly Assembly scriptsAssembly;
        private readonly string scriptsResourcePath;

        public Scripts()
            : this(typeof(Scripts).Assembly, "NBB.EventStore.AdoNet.Internal.SqlScripts")
        {
        }

        protected Scripts(Assembly scriptsAssembly, string scriptsResourcePath)
        {
            this.scriptsAssembly = scriptsAssembly;
            this.scriptsResourcePath = scriptsResourcePath;
        }

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
                    using (Stream stream = scriptsAssembly.GetManifestResourceStream($"{scriptsResourcePath}.{key}.sql"))
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
