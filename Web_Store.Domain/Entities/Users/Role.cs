using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_Store.Domain.Entities.Commons;

namespace Web_Store.Domain.Entities.Users
{
    public class Role : BaseEntity
    {
        public string? Name { get; set; }
        public ICollection<UserInRole>? UserInRoles { get; set; }
    }
}
