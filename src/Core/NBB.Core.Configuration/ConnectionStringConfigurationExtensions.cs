// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Core.Configuration;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Extension methods for building connection strings from configuration.
    /// </summary>
    public static class ConnectionStringConfigurationExtensions
    {
        /// <summary>
        /// Retrieves the connection string with the specified name from the <c>ConnectionStrings</c> section,
        /// either from separate connection info segments or from a string value.
        /// </summary>
        /// <param name="configuration">The configuration to read from.</param>
        /// <param name="name">The connection string name.</param>
        /// <returns>The connection string, or <c>null</c> when it is not configured.</returns>
        /// <exception cref="ConnectionStringConfigurationException">A mandatory connection info segment is missing.</exception>
        public static string? ResolveConnectionString(this IConfiguration configuration, string name)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var section = configuration.GetSection($"ConnectionStrings:{name}");
            if (section.GetChildren().Any())
            {
                return new ConnectionStringBuilder(section).Build();
            }

            return section.Value;
        }

        private sealed class ConnectionStringBuilder(IConfigurationSection section)
        {
            // Authentication modes that use an identity instead of a password (compared ignoring case and spaces)
            private static readonly HashSet<string> PasswordlessAuthenticationModes = new(StringComparer.OrdinalIgnoreCase)
            {
                "ActiveDirectoryWorkloadIdentity",
                "ActiveDirectoryManagedIdentity",
                "ActiveDirectoryMSI",
                "ActiveDirectoryDefault"
            };

            public string? Server { get; } = section[nameof(Server)];
            public string? Database { get; } = section[nameof(Database)];
            public string? UserName { get; } = section[nameof(UserName)];
            public string? Password { get; } = section[nameof(Password)];
            public string? OtherParams { get; } = section[nameof(OtherParams)];
            public string? Authentication { get; } = section[nameof(Authentication)];
            public string? IdentityClientId { get; } = section[nameof(IdentityClientId)];

            public string Build()
            {
                if (string.IsNullOrWhiteSpace(Authentication))
                {
                    Validate();

                    return $"Server={Server};Database={Database};User Id={UserName};Password={Password};{OtherParams}";
                }

                if (IsPasswordlessAuthentication(Authentication))
                {
                    ValidatePasswordless();

                    return $"Server={Server};Database={Database};User Id={IdentityClientId};Authentication={Authentication};{OtherParams}";
                }

                Validate();

                return $"Server={Server};Database={Database};User Id={UserName};Password={Password};Authentication={Authentication};{OtherParams}";
            }

            private static bool IsPasswordlessAuthentication(string authentication)
            {
                var normalized = string.Concat(authentication.Where(c => !char.IsWhiteSpace(c)));
                return PasswordlessAuthenticationModes.Contains(normalized);
            }

            private static void CheckMandatoryField(string? fieldValue, string fieldName)
            {
                if (string.IsNullOrWhiteSpace(fieldValue))
                {
                    throw new ConnectionStringConfigurationException($"Connection string part {fieldName} is not provided!");
                }
            }

            private void Validate()
            {
                CheckMandatoryField(Server, nameof(Server));
                CheckMandatoryField(Database, nameof(Database));
                CheckMandatoryField(UserName, nameof(UserName));
                CheckMandatoryField(Password, nameof(Password));
            }

            private void ValidatePasswordless()
            {
                CheckMandatoryField(Server, nameof(Server));
                CheckMandatoryField(Database, nameof(Database));
                CheckMandatoryField(IdentityClientId, nameof(IdentityClientId));
            }
        }
    }
}

namespace NBB.Core.Configuration
{
    /// <summary>
    /// Exception thrown when a connection string is not properly configured
    /// </summary>
    public class ConnectionStringConfigurationException(string message) : Exception(message);
}
