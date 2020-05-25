using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;

namespace NBB.Tools.Reflection
{
    /// <summary>
    /// Utility class used to create objects and types at runtime
    /// </summary>
    public static class CustomTypeBuilder
    {
        /// <summary>
        /// Dictionary to store the list of created types
        /// </summary>
        private static readonly ConcurrentDictionary<string, Type> _cache = new ConcurrentDictionary<string, Type>();
        /// <summary>
        /// lock object
        /// </summary>
        private static readonly object _lockObject = new object();
        /// <summary>
        /// Default module name in which new types will be created
        /// </summary>
        public readonly static string ModuleName = "MainModule";

        /// <summary>
        /// Function that will be used to create type names. You can override class name generation
        /// </summary>
        public static Func<string, string> TypeNameCreator => (name) => string.IsNullOrEmpty(name) ? Guid.NewGuid().ToString() : name;

        /// <summary>
        /// Will create a new object and a new type, based on the provided field definition
        /// </summary>
        /// <param name="fields">The list of fields</param>
        /// <param name="name">Class name</param>
        /// <param name="module">Name of module in which the type will be created</param>
        /// <param name="useCache">If a type with the same definition was created, it can be retrieved from cache. Default: true</param>
        /// <returns>Created object</returns>
        public static object CreateNewObject(List<CustomFieldDefinition> fields, string name = "", string module = "", bool useCache = true)
        {
            var myType = CompileResultType(fields, name, module, useCache);
            var myObject = Activator.CreateInstance(myType);
            return myObject;
        }

        /// <summary>
        /// Creates a new type
        /// </summary>
        /// <param name="fields">The list of fields</param>
        /// <param name="name">Class name</param>
        /// <param name="module">Name of module in which the type will be created</param>
        /// <param name="useCache">If a type with the same definition was created, it can be retrieved from cache. Default: true</param>
        /// <returns>Created type</returns>
        public static Type CompileResultType(List<CustomFieldDefinition> fields, string name = "", string module = "", bool useCache = true)
        {
            string hash = string.Empty;
            lock (_lockObject)
            {
                if (useCache)
                {
                    hash = CalculateFieldHash(fields);
                    if (_cache.ContainsKey(hash))
                        return _cache[hash];
                }

                TypeBuilder tb = GetTypeBuilder(name, module);
                ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

                // NOTE: assuming your list contains Field objects with fields FieldName(string) and FieldType(Type)
                foreach (var field in fields)
                    CreateProperty(tb, field.FieldName, field.FieldType);

                Type objectType = tb.CreateType();
                if (useCache)
                    _cache[hash] = objectType;
                return objectType;
            }
        }

        /// <summary>
        /// Calculates the MD5 Hash of the fields
        /// </summary>
        /// <param name="fields">List of fields</param>
        /// <returns>Hash of the list of fields</returns>
        private static string CalculateFieldHash(List<CustomFieldDefinition> fields)
        {
            var sb = new StringBuilder();
            var clone = new List<CustomFieldDefinition>();
            clone.AddRange(fields);
            clone.Sort((a1, a2) => a1.FieldName.CompareTo(a2.FieldName));
            foreach (var field in clone)
            {
                sb.Append(field.FieldName);
                sb.Append(field.FieldType.FullName);
            }
            return CalculateMD5Hash(sb.ToString());
        }

        /// <summary>
        /// Calculates MD5 hash of a string
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns>MD5 hash of the string</returns>
        private static string CalculateMD5Hash(string input)
        {
            StringBuilder sb = new StringBuilder();

            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hash = md5.ComputeHash(inputBytes);

                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Creates a TypeBuilder
        /// </summary>
        /// <param name="name"></param>
        /// <param name="module"></param>
        /// <returns>The TypeBuilder</returns>
        private static TypeBuilder GetTypeBuilder(string name = "", string module = "")
        {
            var typeSignature = TypeNameCreator(name);
            var an = new AssemblyName(typeSignature);

            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(string.IsNullOrWhiteSpace(module) ? ModuleName : module);
            TypeBuilder tb = moduleBuilder.DefineType(typeSignature,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    null);
            return tb;
        }

        /// <summary>
        /// Adds a property to a type
        /// </summary>
        /// <param name="tb">The type builder</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="propertyType">Property value</param>
        private static void CreateProperty(TypeBuilder tb, string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            PropertyBuilder propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr =
                tb.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });

            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            Label modifyProperty = setIl.DefineLabel();
            Label exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }
    }

    /// <summary>
    /// Used to pass field definitions to CustomTypeBuilder
    /// </summary>
    public class CustomFieldDefinition
    {
        /// <summary>
        /// Name of the field
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// Type of the field
        /// </summary>
        /// <value>The type</value>
        public Type FieldType { get; set; }
    }
}