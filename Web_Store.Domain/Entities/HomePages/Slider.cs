using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_Store.Domain.Entities.Commons;

namespace Web_Store.Domain.Entities.HomePages
{
    public class Slider : BaseEntity
    {
        public string Src { get; set; }
        public string link { get; set; }
    }
}
