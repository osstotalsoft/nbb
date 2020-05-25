using BenchmarkDotNet.Running;

namespace MessagingBenchmarks
{
    class Program
    {
        protected Program()
        {

        }

        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<MessagingPublisherBenchmark>();
            //var b = new MessagingSubscriberBenchmark();
            //b.KafkaGlobalSetup();
            //b.KafkaSubcribeTest().Wait();
        }
    }
}
