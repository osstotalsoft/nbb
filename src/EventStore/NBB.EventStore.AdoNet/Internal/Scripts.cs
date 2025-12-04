// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;

namespace NBB.EventStore.AdoNet.Internal
{
    public class Scripts
    {
        private readonly ConcurrentDictionary<string, string> _scripts = new();
        private readonly Assembly _scriptsAssembly;
        private readonly string _scriptsResourcePath;

        public Scripts()
            : this(typeof(Scripts).Assembly, "NBB.EventStore.AdoNet.Internal.SqlScripts")
        {
        }

        protected Scripts(Assembly scriptsAssembly, string scriptsResourcePath)
        {
            this._scriptsAssembly = scriptsAssembly;
            this._scriptsResourcePath = scriptsResourcePath;
        }

        public string AppendEventsToStreamExpectedVersion => GetScript(nameof(AppendEventsToStreamExpectedVersion));
        public string AppendEventsToStreamExpectedVersionAny => GetScript(nameof(AppendEventsToStreamExpectedVersionAny));
        public string CreateDatabaseObjects => GetScript(nameof(CreateDatabaseObjects));
        public string DeleteStream => GetScript(nameof(DeleteStream));
        public string DropDatabaseObjects => GetScript(nameof(DropDatabaseObjects));
        public string GetEventsFromStream => GetScript(nameof(GetEventsFromStream));
        public string GetSnapshotForStream => GetScript(nameof(GetSnapshotForStream));
        public string SetSnapshotForStream => GetScript(nameof(SetSnapshotForStream));

        private string GetScript(string name)
        {
            return _scripts.GetOrAdd(name,
                key =>
                {
                    using var stream = _scriptsAssembly.GetManifestResourceStream($"{_scriptsResourcePath}.{key}.sql");
                    if (stream == null)
                    {
                        throw new Exception($"Embedded resource, {name}, not found.");
                    }

                    using var reader = new StreamReader(stream);
                    return reader
                        .ReadToEnd();
                });
        }
    }
}
