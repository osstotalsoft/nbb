using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NBB.Messaging.DataContracts;
using NBB.Core.Abstractions;

namespace NBB.Messaging.Abstractions
{
    public class DefaultMessageTypeRegistry : IMessageTypeRegistry
    {
        //private readonly IEnumerable<Assembly> _scannedAssemblies;
        private readonly ConcurrentDictionary<string, Type> _typeCache = new ConcurrentDictionary<string, Type>();

        /*public DefaultMessageTypeRegistry(IEnumerable<Assembly> scannedAssemblies)
        {
            _scannedAssemblies = scannedAssemblies ?? new List<Assembly>();
            _scannedAssemblies = _scannedAssemblies.Append(typeof(MessagingResponse<>).Assembly);
        }
        */

        public Type ResolveType(string messageTypeId, IEnumerable<Assembly> scannedAssemblies)
        {
            var scannedAssembliesList = scannedAssemblies?.ToList() ?? new List<Assembly>();
            scannedAssembliesList.Add(typeof(MessagingResponse<>).Assembly);

            return _typeCache.GetOrAdd(messageTypeId, key =>
                GetTypeByAttribute(key, scannedAssembliesList) ??
                GetTypeByName(key, scannedAssembliesList) ??
                throw new ApplicationException($"Type could not be resolved: {key}")
            );
        }

        public string GetTypeId(Type messageType)
        {
            return messageType.GetCustomAttribute<MessageTypeIdAttribute>()?.MessageTypeId
                   ?? TypeInfo.Build(messageType).ToString();
        }

        private static string GetTypeNameIncludingNesting(Type type)
        {
            return type.DeclaringType == null ? 
                type.Name : 
                $"{GetTypeNameIncludingNesting(type.DeclaringType)}{TypeInfo.NestedSeparator}{type.Name}";
        }

        private Type GetTypeByName(string messageTypeId, List<Assembly> scannedAssemblies)
        {
            var parsedType = TypeInfo.Parse(messageTypeId);

            return GetTypeByNameInternal(parsedType, scannedAssemblies);
        }

        private Type GetTypeByNameInternal(TypeInfo parsedType, List<Assembly> scannedAssemblies)
        {
            var dotnetFormattedName = parsedType.GetNameForDotnetComparison();
            var foundType = WellKnownTypes.Resolve(dotnetFormattedName);
            if (foundType != null)
                return foundType;

            var foundTypes = scannedAssemblies
                .SelectMany(x => x.ExportedTypes)
                .Where(x => GetTypeNameIncludingNesting(x) == dotnetFormattedName)
                .ToList();

            if (foundTypes.Count > 1)
                throw new ApplicationException($"Multiple types found with name: {foundTypes.First().GetPrettyName()}");

            foundType = foundTypes.FirstOrDefault();
            if (foundType == null)
                return null;

            if (parsedType.IsGeneric && parsedType.TypeArguments.Any())
            {
                var foundTypeArgs = new List<Type>();
                foreach (var parsedTypeTypeArgument in parsedType.GetAllTypeArguments())
                {
                    var foundTypeArgument = GetTypeByNameInternal(parsedTypeTypeArgument, scannedAssemblies);
                    if (foundTypeArgument == null)
                        return null;

                    foundTypeArgs.Add(foundTypeArgument);
                }

                foundType = foundType.MakeGenericType(foundTypeArgs.ToArray());
            }

            return parsedType.ArrayDimensions.AsEnumerable()
                .Reverse()
                .Aggregate(foundType, (current, arrayDimension) => arrayDimension.Dimensions == 1
                    ? current.MakeArrayType()
                    : current.MakeArrayType(arrayDimension.Dimensions));
        }

        private Type GetTypeByAttribute(string messageTypeId, List<Assembly> scannedAssemblies)
        {
            var foundTypes = scannedAssemblies
                .SelectMany(x => x.ExportedTypes)
                .Where(x => x.GetCustomAttribute<MessageTypeIdAttribute>()?.MessageTypeId == messageTypeId)
                .ToList();

            if (foundTypes.Count > 1)
                throw new ApplicationException(
                    $"Multiple types found with the same MessageTypeIdAttribute: {messageTypeId}");

            return foundTypes.FirstOrDefault();
        }

        private class TypeInfo
        {
            public const char NestedSeparator = '+';
            private string _name;

            private string Name
            {
                get => _name;
                set => _name = _name ?? value?.Trim();
            }

            public bool IsGeneric { get; private set; }
            public List<ArrayDimension> ArrayDimensions { get; private set; }
            public List<TypeInfo> TypeArguments { get; private set; }
            private TypeInfo DeclaringTypeInfo { get; set; }

            public class ArrayDimension
            {
                public int Dimensions { get; set; }

                public ArrayDimension()
                {
                    Dimensions = 1;
                }

                public override string ToString()
                {
                    return $"[{new string(',', Dimensions - 1)}]";
                }
            }

            private TypeInfo()
            {
                Name = null;
                IsGeneric = false;
                ArrayDimensions = new List<ArrayDimension>();
                TypeArguments = new List<TypeInfo>();
            }

            public override string ToString()
            {
                var str = new StringBuilder();
                if (DeclaringTypeInfo != null)
                {
                    str.Append(DeclaringTypeInfo).Append(NestedSeparator);
                }

                str.Append(Name);
                if (IsGeneric)
                    str.Append($"<{string.Join(",", TypeArguments.Select(tn => tn.ToString()))}>");

                foreach (var d in ArrayDimensions)
                    str.Append(d);

                return str.ToString();
            }

            public string GetNameForDotnetComparison()
            {
                var formattedExpectedTypeName = IsGeneric ? $"{Name}`{TypeArguments.Count}" : Name;

                return DeclaringTypeInfo != null
                    ? $"{DeclaringTypeInfo.GetNameForDotnetComparison()}{NestedSeparator}{formattedExpectedTypeName}"
                    : formattedExpectedTypeName;
            }

            public List<TypeInfo> GetAllTypeArguments()
            {
                var allArguments = new List<TypeInfo>();
                if (DeclaringTypeInfo != null)
                {
                    allArguments.AddRange(DeclaringTypeInfo.GetAllTypeArguments());
                }
                allArguments.AddRange(TypeArguments);

                return allArguments;
            }

            public static TypeInfo Parse(string name)
            {
                var pos = 0;
                return ParseInternal(name, ref pos, out _);
            }

            public static TypeInfo Build(Type type)
            {
                if (type.IsArray)
                {
                    var elementTypeName = Build(type.GetElementType());
                    elementTypeName.ArrayDimensions.Add(
                        new ArrayDimension { Dimensions = type.GetArrayRank() });
                    elementTypeName.ArrayDimensions.Reverse();
                    return elementTypeName;
                }

                var typeName = new TypeInfo
                {
                    Name = type.IsGenericType ? type.Name.Substring(0, type.Name.IndexOf('`')) : type.Name,
                    IsGeneric = type.IsGenericType,
                    TypeArguments = type.GenericTypeArguments
                        .Select(Build).ToList(),
                };

                if (type.DeclaringType != null)
                {
                    typeName.DeclaringTypeInfo = BuildDeclaringTypeName(type, typeName);
                }

                return typeName;
            }

            private static TypeInfo BuildDeclaringTypeName(Type type, TypeInfo typeInfo)
            {
                if (type.DeclaringType == null)
                    return null;

                var declaringTypeName = Build(type.DeclaringType);
                var declaringTypeGenericParamsCount = ((System.Reflection.TypeInfo)type.DeclaringType).GenericTypeParameters.Length;
                declaringTypeName.TypeArguments.AddRange(typeInfo.TypeArguments.GetRange(0, declaringTypeGenericParamsCount));
                typeInfo.TypeArguments.RemoveRange(0, declaringTypeGenericParamsCount);

                return declaringTypeName;
            }

            private static TypeInfo ParseInternal(string name, ref int index, out bool listTerminated)
            {
                var nameBuilder = new StringBuilder();
                var typeName = new TypeInfo();
                listTerminated = true;

                while (index < name.Length)
                {
                    char currentChar = name[index++];
                    switch (currentChar)
                    {
                        case ',':
                            typeName.Name = nameBuilder.ToString();
                            listTerminated = false;
                            return typeName;
                        case '>':
                            typeName.Name = nameBuilder.ToString();
                            listTerminated = true;
                            return typeName;
                        case NestedSeparator:
                            typeName.Name = nameBuilder.ToString();
                            typeName = new TypeInfo {DeclaringTypeInfo = typeName};
                            nameBuilder.Clear();
                            break;
                        case '<':
                            {
                                typeName.Name = nameBuilder.ToString();
                                typeName.IsGeneric = true;
                                nameBuilder.Clear();

                                bool terminated = false;
                                while (!terminated)
                                {
                                    typeName.TypeArguments.Add(ParseInternal(name, ref index, out terminated));
                                }

                                if (name[index - 1] != '>')
                                    throw new Exception("Missing closing > of generic type list.");

                                break;
                            }
                        case '[':
                            var arrayDimension = new ArrayDimension();
                            typeName.ArrayDimensions.Add(arrayDimension);

                            if (index >= name.Length)
                                throw new Exception("Expecting ] or , after [ for array type, but reached end of string.");

                            bool arrayTerminated = false;
                            while (index < name.Length && !arrayTerminated)
                            {
                                currentChar = name[index++];
                                switch (currentChar)
                                {
                                    case ']':
                                        arrayTerminated = true;//array specifier terminated
                                        break;
                                    case ',': //multidimensional array
                                        arrayDimension.Dimensions++;
                                        break;
                                    default:
                                        throw new Exception($@"Expecting ""]"" or "","" after ""["" for array specifier but encountered ""{currentChar}"".");
                                }
                            }
                            break;
                        default:
                            nameBuilder.Append(currentChar);
                            continue;
                    }
                }

                typeName.Name = nameBuilder.ToString();
                return typeName;
            }
        }

        private static class WellKnownTypes
        {
            private static readonly Dictionary<string, Type> Map = new Dictionary<string, Type>
            {
                ["bool"] = typeof(bool),
                ["Boolean"] = typeof(bool),
                ["byte"] = typeof(byte),
                ["Byte"] = typeof(byte),
                ["sbyte"] = typeof(sbyte),
                ["SByte"] = typeof(sbyte),
                ["char"] = typeof(char),
                ["Char"] = typeof(char),
                ["decimal"] = typeof(decimal),
                ["Decimal"] = typeof(decimal),
                ["double"] = typeof(double),
                ["Double"] = typeof(decimal),
                ["float"] = typeof(float),
                ["Float"] = typeof(float),
                ["int"] = typeof(int),
                ["Int32"] = typeof(int),
                ["uint"] = typeof(uint),
                ["UInt32"] = typeof(uint),
                ["long"] = typeof(long),
                ["Int64"] = typeof(long),
                ["ulong"] = typeof(ulong),
                ["UInt64"] = typeof(ulong),
                ["object"] = typeof(object),
                ["Object"] = typeof(object),
                ["short"] = typeof(short),
                ["Int16"] = typeof(short),
                ["ushort"] = typeof(ushort),
                ["UInt16"] = typeof(ushort),
                ["short"] = typeof(short),
                ["Int16"] = typeof(short),
                ["string"] = typeof(string),
                ["String"] = typeof(string),

                ["List`1"] = typeof(List<>),
                ["Dictionary`2"] = typeof(Dictionary<,>)
            };

            public static Type Resolve(string typeName)
            {
                return Map.TryGetValue(typeName, out var type) ? type : null;
            }
        }
    }
}
