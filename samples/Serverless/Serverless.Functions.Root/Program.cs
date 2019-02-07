using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NBB.Contracts.Functions.CreateContract;
using Serverless.Functions.Root.Lib;

namespace Serverless.Functions.Root
{
    class Program
    {

        static void Main(string[] args)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                source.Cancel();
            };

            MainAsync(source.Token).GetAwaiter().GetResult();
        }

        static async Task MainAsync(CancellationToken token)
        {
            var function = new Function();
            function.PrepareFunctionContext();

            System.Diagnostics.Debug.WriteLine("C# AfterBurn running.");

            var httpFormatter = new HttpFormatter();
            var stdin = Console.OpenStandardInput();
            using (TextReader reader = new StreamReader(stdin))
            {
                while (!token.IsCancellationRequested)
                {
                    HeaderParser parser = new HeaderParser();
                    var header = parser.Parse(reader);

                    foreach (string v in header.HttpHeaders)
                    {
                        System.Diagnostics.Debug.WriteLine(v + "=" + header.HttpHeaders[v]);
                    }

                    System.Diagnostics.Debug.WriteLine("Content-Length: " + header.ContentLength);

                    BodyParser bodyParser = new BodyParser();
                    string body = String.Empty;
                    if (header.ContentLength > 0)
                    {
                        body = bodyParser.Parse(reader, header.ContentLength);

                        System.Diagnostics.Debug.WriteLine(body);
                    }

                    await function.Invoke(body, token);

                    var httpAdded = httpFormatter.Format("");
                    Console.WriteLine(httpAdded);
                }
            }
        }
    }
}