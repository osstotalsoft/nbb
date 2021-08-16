// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using AutoFixture;
using System;

namespace NBB.Application.DataContracts.Schema.Sample
{
    public class SampleBuilder
    {
        public T GetSample<T>() where T : class
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new StringSpecimenBuilder());

            var sample = fixture.Create<T>();
            return sample;

        }

        public object GetSampleFromType(Type type)
        {
            var method = typeof(SampleBuilder).GetMethod("GetSample");

            var genericMethod = method.MakeGenericMethod(type);
            var obj = genericMethod.Invoke(this, null); 
            return obj;
        }
    }
}
