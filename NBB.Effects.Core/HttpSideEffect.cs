namespace NBB.Effects.Core
{
    class HttpSideEffect<T> : ISideEffect<string>
    {
        public string Url { get; }

        public HttpSideEffect(string url)
        {
            Url = url;
        }
    }
}
