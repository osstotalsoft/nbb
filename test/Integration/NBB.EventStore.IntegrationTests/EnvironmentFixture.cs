//using System;
//using Xunit;

//[assembly: CollectionBehavior(DisableTestParallelization = true)]
//namespace NBB.EventStore.IntegrationTests
//{
//    public class EnvironmentFixture : IDisposable
//    {
//        private const string EnvironmentKey = "NETCORE_ENVIRONMENT";
//        private readonly string _initialEnvironment;
//        public EnvironmentFixture()
//        {
//            _initialEnvironment = Environment.GetEnvironmentVariable(EnvironmentKey);
//            var isDevelopment = string.Equals(_initialEnvironment, "development", StringComparison.OrdinalIgnoreCase);

//            if (!isDevelopment)
//            {
//                Environment.SetEnvironmentVariable(EnvironmentKey, "Development");
//            }
//        }

//        public void Dispose()
//        {
//            Environment.SetEnvironmentVariable(EnvironmentKey, _initialEnvironment);
//        }
//    }
//}
