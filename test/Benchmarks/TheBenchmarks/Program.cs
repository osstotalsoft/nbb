using System;
using BenchmarkDotNet.Running;
namespace TheBenchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //var summary = BenchmarkRunner.Run<EventStoreBenchmark>();
            var summary = BenchmarkRunner.Run<EventSourcedRepositoryBenchmark>();

            //var benchmark = new EventSourcedRepositoryBenchmark();
            //benchmark.NumberOfEvents = 1;
            //benchmark.GlobalSetupLoadAndSaveAggregateWithoutSnapshot();
            //benchmark.LoadAndSaveAggregateWithoutSnapshot();
        }
    }
}
