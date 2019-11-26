namespace NBB.Core.Effects
{
    class HttpSideEffect<T> : ISideEffect<string>
    {
        public string Url { get; }

        public HttpSideEffect(string url)
        {
            Url = url;
        }
    }

    class ProcessManagerDefinition
    {
        static IEffect<string> GetHtpp(string url)
        {
            return Effect.Of(new HttpSideEffect<string>(url));
        }

        static IEffect<bool> Main()
        {
            //GetHtpp("google")
            //    .Then(x=> x.ToUpper())
            //    .Then(x=> GetHtpp("microsoft"+x))
            //    .Then(x => Effect.Parallel(
            //        GetHtpp(""),
            //        GetHtpp("")
            //    ))


            return LoadFromDb(1).Then(DomainHandle).Then(SaveToDb);

        }


        static IEffect<string> LoadFromDb(int id)
        {
            return new PureEffect<string>("ceva");
        }

        static IEffect<bool> SaveToDb(string entity)
        {
            return new PureEffect<bool>(true);
        }

        static string DomainHandle(string domain)
        {
            return domain;
        }


    }
}
