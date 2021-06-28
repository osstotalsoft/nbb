using System;
using System.Linq;
using MediatR;

namespace NBB.ProjectR
{
    public interface OneOf: INotification { }
    public interface OneOf<T1> : OneOf { }
    public interface OneOf<T1, T2> : OneOf { }
    public interface OneOf<T1, T2, T3> : OneOf { }
    public interface OneOf<T1, T2, T3, T4> : OneOf { }

    public static class OneOfExtensions
    {
        public static bool IsOneOfType(this Type t) =>
            typeof(OneOf).IsAssignableFrom(t);

        public static Type[] GetSumOfTypes(this Type t)
        {
            if (!t.IsOneOfType())
                return Array.Empty<Type>();

            var types =
                t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.IsOneOfType())
                    .SelectMany(i => i.GetGenericArguments())
                    .Distinct()
                    .ToArray();

            return types;
        }
    }

}
