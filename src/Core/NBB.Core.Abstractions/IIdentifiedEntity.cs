namespace NBB.Core.Abstractions
{
    public interface IIdentifiedEntity
    {
        object GetIdentityValue();
    }

    public static class IdentifiedEntityExtensions
    {
        public static string GetTypeId(this IIdentifiedEntity entity)
        {
            return entity.GetType().FullName;
        }
    }
}
