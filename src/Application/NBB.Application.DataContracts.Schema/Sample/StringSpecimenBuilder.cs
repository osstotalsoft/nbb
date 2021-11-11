using System;
using System.Linq;
using System.Reflection;
using AutoFixture.Kernel;

namespace NBB.Application.DataContracts.Schema.Sample
{
    public class StringSpecimenBuilder : ISpecimenBuilder
    {
        private readonly Random random = new Random();
        
        public string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstwyxzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public object Create(object request, ISpecimenContext context)
        {
            if (request is PropertyInfo pi && pi.PropertyType == typeof(string))
            {
                return RandomString(10);
            }

            return new NoSpecimen();
        }
    }
}
