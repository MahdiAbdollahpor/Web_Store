using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_Store.Domain.Entities.Commons;
using Web_Store.Domain.Entities.Products;

namespace Web_Store.Domain.Entities.Orders
{
    public class OrderDetail : BaseEntity
    {
        public virtual Order? Order { get; set; }
        public long OrderId { get; set; }

        public virtual Product? Product { get; set; }
        public long ProductId { get; set; }

        public int Price { get; set; }
        public int Count { get; set; }
    }
}
