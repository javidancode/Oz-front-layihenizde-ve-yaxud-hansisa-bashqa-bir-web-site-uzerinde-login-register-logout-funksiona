using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Entity_Frame_Work_Project.ViewModels.ProductViewModel
{
    public class ProductListVM 
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public Decimal Price { get; set; }
        public string Desc { get; set; }
        public string CategoryName { get; set; }
        public string MainImage { get; set; }
    }
}
