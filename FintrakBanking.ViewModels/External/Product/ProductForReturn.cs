using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Product
{
    public class ProductForReturn
    {
        public int productId { get; set; }    
        public string productCode { get; set; }
        public string productName { get; set; }
        public string productDescription { get; set; }
        public int? productTenor { get; set; }
        public int? maxProductTenor { get; set; }
        public int? minProductTenor { get; set; }
        public decimal? maxProductAmount { get; set; }
        public decimal? minProductAmount { get; set; }
    }
}
