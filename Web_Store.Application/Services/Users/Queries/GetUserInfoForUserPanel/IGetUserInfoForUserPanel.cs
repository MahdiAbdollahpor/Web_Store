using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_Store.Application.Interfaces.Contexts;

namespace Web_Store.Application.Services.Users.Queries.GetUserInfoForUserPanel
{
    public interface IGetUserInfoForUserPanel
    {
        UserInfoDtoForDashbord Execute (long userId);
    }

    public class GetUserInfoForUserPanel : IGetUserInfoForUserPanel
    {
        private readonly IDataBaseContext _context;

        public GetUserInfoForUserPanel(IDataBaseContext context)
        {
            _context = context;
        }

        public UserInfoDtoForDashbord Execute(long userId)
        {
            var userInfo = _context.Users.FirstOrDefault( p => p.Id == userId);

            

            return new UserInfoDtoForDashbord
            {
                id = userInfo.Id,
                fullName = userInfo.FullName,
                Email = userInfo.Email

            };
        }
    }

    public class UserInfoDtoForDashbord
    {
        public long id { get; set; }
        public string fullName { get; set; }
        public string Email { get; set; }
    }
}
