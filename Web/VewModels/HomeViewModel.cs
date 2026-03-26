using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.VewModels
{
    public class HomeViewModel
    {
        public Owner Owner { get; set;}
        public List<PortofolioItem>PortfolioItems { get; set; }  
    }
}
