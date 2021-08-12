// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AutoFixture;
using AutoFixture.Kernel;

namespace NBB.Application.DataContracts.Schema.Sample
{
    public class StringSpecimenBuilder : ISpecimenBuilder
    {
        private Random random = new Random();
        
        public string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstwyxzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public object Create(object request, ISpecimenContext context)
        {
            if (request is PropertyInfo pi)
            {
                if (pi.PropertyType == typeof(string))
                {
                    return RandomString(10);
                }
            }

            if (request is ParameterInfo pi2)
            {
                if (pi2.ParameterType == typeof(string))
                {
                    return RandomString(10);
                }
            }
            return new NoSpecimen();
        }
    }
}
