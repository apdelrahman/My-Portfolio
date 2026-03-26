using System;
using System.Collections.Generic;

namespace Core.Entities
{
    public class PortfolioImage : Entitybase
    {
        public string ImageUrl { get; set; }
        public Guid PortofolioItemId { get; set; } 
        public PortofolioItem PortofolioItem { get; set; }
    }
    public class PortofolioItem : Entitybase
    {
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public virtual ICollection<PortfolioImage> ProjectImages { get; set; } = new List<PortfolioImage>();
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }
}