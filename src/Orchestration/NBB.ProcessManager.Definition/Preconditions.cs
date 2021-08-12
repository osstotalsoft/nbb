﻿// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace NBB.ProcessManager.Definition
{
    [DebuggerStepThrough]
    public static class Preconditions
    {
        [ContractAnnotation("value:null => halt")]
        public static T NotNull<T>([NoEnumeration] T value, [InvokerParameterName, NotNull] string parameterName)
            where T : class
        {
            if (ReferenceEquals(value, null))
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static string NotEmpty(string value, [InvokerParameterName, NotNull] string parameterName)
        {
            if (ReferenceEquals(value, null))
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentNullException(parameterName);
            }

            if (value.Length == 0)
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException("String value cannot be null.", parameterName);
            }

            return value;
        }

        public static TEnum IsDefined<TEnum>(TEnum value, [InvokerParameterName, NotNull] string parameterName) where TEnum : struct
        {
            if (!Enum.IsDefined(typeof(TEnum), value))
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentOutOfRangeException(parameterName);
            }

            return value;
        }
    }
}
