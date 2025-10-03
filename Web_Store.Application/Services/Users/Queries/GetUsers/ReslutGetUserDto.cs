using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web_Store.Application.Services.Users.Queries.GetUsers
{
    public class ReslutGetUserDto
    {
        public List<GetUsersDto> Users { get; set; }
        public int Rows { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)Rows / PageSize);
    }
}
