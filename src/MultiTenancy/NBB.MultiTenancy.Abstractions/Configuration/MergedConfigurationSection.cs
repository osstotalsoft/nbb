// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NBB.MultiTenancy.Abstractions.Configuration
{
    public class MergedConfigurationSection : IConfigurationSection
    {
        private readonly IConfigurationSection _innerConfigurationSection;
        private readonly IConfigurationSection _defaultConfigurationSection;

        public MergedConfigurationSection(IConfigurationSection innerConfigurationSection, IConfigurationSection defaultConfigurationSection)
        {
            _innerConfigurationSection = innerConfigurationSection ?? throw new ArgumentNullException(nameof(innerConfigurationSection));
            _defaultConfigurationSection = defaultConfigurationSection ?? throw new ArgumentNullException(nameof(defaultConfigurationSection));

        }
        public string this[string key]
        {
            get => _innerConfigurationSection[key] ?? _defaultConfigurationSection[key];
            set => _innerConfigurationSection[key] = value;
        }

        public string Key => _innerConfigurationSection.Key;

        public string Path => _innerConfigurationSection.Path;

        public string Value
        {
            get => _innerConfigurationSection.Value ?? (!_innerConfigurationSection.GetChildren().Any() ? _defaultConfigurationSection.Value : null);
            set => _innerConfigurationSection.Value = value;
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            var innerChildren = _innerConfigurationSection.GetChildren().ToDictionary(s => s.Key);
            if(innerChildren.Count == 0 && _innerConfigurationSection.Value is not null)
            {
                return innerChildren.Values;
            }

            foreach (var c in _defaultConfigurationSection.GetChildren())
            {
                if (innerChildren.ContainsKey(c.Key))
                {
                    innerChildren[c.Key] = new MergedConfigurationSection(innerChildren[c.Key], c);
                }
                else
                {
                    innerChildren[c.Key] = c;
                }
            }
            return innerChildren.Values;
        }

        public IChangeToken GetReloadToken()
        {
            return _innerConfigurationSection.GetReloadToken();
        }

        public IConfigurationSection GetSection(string key)
        {
            //config.GetSection is never null
            var innerCfg = _innerConfigurationSection.GetSection(key);
            var innerCfgIsEmpty = innerCfg.Value is null && !innerCfg.GetChildren().Any();
            var defaultCfg = _defaultConfigurationSection.GetSection(key);
            var defaultCfgIsEmpty = defaultCfg.Value is null && !defaultCfg.GetChildren().Any();
            var result = (innerCfgIsEmpty, defaultCfgIsEmpty) switch
            {
                (true, false) => defaultCfg,
                (_, true) => innerCfg,
                _ => new MergedConfigurationSection(innerCfg, defaultCfg)
            };
            return result;
        }
    }
}
