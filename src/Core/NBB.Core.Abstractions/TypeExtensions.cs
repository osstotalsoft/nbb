// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Linq;
using System.Text;

namespace NBB.Core.Abstractions
{
    public static class TypeExtensions
    {
        public static string GetPrettyName( this Type type)
        {
            var retType = new StringBuilder();

            if (type.IsGenericType)
            {
                var parentTypeName = type.FullName?.Split('`')[0].Split('.').Last();
                // We will build the type here.
                Type[] arguments = type.GetGenericArguments();

                var argList = new StringBuilder();
                foreach (Type t in arguments)
                {
                    // Let's make sure we get the argument list.
                    string arg = t.GetPrettyName();
                    if (argList.Length > 0)
                    {
                        argList.AppendFormat(", {0}", arg);
                    }
                    else
                    {
                        argList.Append(arg);
                    }
                }

                if (argList.Length > 0)
                {
                    retType.AppendFormat("{0}<{1}>", parentTypeName, argList.ToString());
                }
            }
            else
            {
                return type.FullName?.Split('.').Last() ?? type.Name;
            }

            return retType.ToString();
        }

        public static string GetLongPrettyName( this Type type)
        {
            var retType = new StringBuilder();

            if (type.IsGenericType)
            {
                var parentTypeName = type.FullName?.Split('`')[0];
                // We will build the type here.
                Type[] arguments = type.GetGenericArguments();

                var argList = new StringBuilder();
                foreach (Type t in arguments)
                {
                    // Let's make sure we get the argument list.
                    string arg = t.GetLongPrettyName();
                    if (argList.Length > 0)
                    {
                        argList.AppendFormat(", {0}", arg);
                    }
                    else
                    {
                        argList.Append(arg);
                    }
                }

                if (argList.Length > 0)
                {
                    retType.AppendFormat("{0}<{1}>", parentTypeName, argList.ToString());
                }
            }
            else
            {
                return type.FullName;
            }

            return retType.ToString();
        }
    }
}
