namespace CleanApp.Contracts
{
    public class PageOf<T> where T : class
    {
        public List<T> List { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
    }
}
