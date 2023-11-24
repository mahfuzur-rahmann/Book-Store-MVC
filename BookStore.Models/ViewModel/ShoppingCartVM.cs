using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModel
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> CartList { get; set; }

        //public double CartTotalPrice { get; set; }

        public OrderHeader OrderHeader { get; set; }
    }
}
 










