namespace Api.Helpers
{
    public class QueryObject
    {
        private const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 20;
        private const int MaxPageSize = 100;

        private int _pageNumber = DefaultPageNumber;
        private int _pageSize = DefaultPageSize;

        public string? Symbol { get; set; } = null;
        public string? CompanyName { get; set; } = null;
        public string? Industry { get; set; } = null;
        public string? SortBy { get; set; } = null;
        public bool IsDescending { get; set; } = false;

        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value <= 0 ? DefaultPageNumber : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (value <= 0)
                {
                    _pageSize = DefaultPageSize;
                }
                else if (value > MaxPageSize)
                {
                    _pageSize = MaxPageSize;
                }
                else
                {
                    _pageSize = value;
                }
            }
        }
    }
}