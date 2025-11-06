using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_Store.Domain.Entities.Commons;
using Web_Store.Domain.Entities.Users;

namespace Web_Store.Domain.Entities.Carts
{
    public class Cart : BaseEntity
    {
        
        public virtual User? User { get; set; }
        public long? UserId { get; set; }

        public Guid BrowserId { get; set; }
        public bool Finished { get; set; }
        public ICollection<CartItem>? CartItems { get; set; }
    }
}
