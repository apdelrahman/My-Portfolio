using Core.Entities;
using System.Collections.Generic;

namespace Web.ViewModels
{
    public class DashboardViewModel
    {
        public Owner OwenerInfo { get; set; }
        public List<PortofolioItem> PortfolioItems { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int UnreadMessagesCount { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}