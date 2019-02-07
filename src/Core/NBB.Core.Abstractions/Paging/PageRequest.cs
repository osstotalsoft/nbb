namespace NBB.Core.Abstractions.Paging
{
    public class PageRequest
    {
        public int Page { get; }
        public int PageSize { get; }

        public PageRequest(int page, int pageSize)
        {
            Page = page;
            PageSize = pageSize;
        }
    }
}
