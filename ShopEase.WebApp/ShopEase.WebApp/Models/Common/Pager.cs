namespace ShopEase.WebApp.Models.Common
{
    public class Pager
    {
        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalItems { get; set; }

        public int TotalPages
        {
            get
            {
                if (PageSize == 0)
                    return 0;

                return (int)Math.Ceiling((double)TotalItems / PageSize);
            }
        }

        public bool HasPreviousPage => CurrentPage > 1;

        public bool HasNextPage => CurrentPage < TotalPages;
    }
}