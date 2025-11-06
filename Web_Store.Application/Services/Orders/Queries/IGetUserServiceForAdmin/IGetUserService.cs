using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_Store.Application.Interfaces.Contexts;
using Web_store.Common.Dto;

namespace Web_Store.Application.Services.Orders.Queries.IGetUserServiceForAdmin
{
    public interface IGetUserService
    {
        ResultDto<UserOrderDto> Execute(long userId);
    }

    public class GetUserService : IGetUserService
    {
        private readonly IDataBaseContext _context;

        public GetUserService(IDataBaseContext context)
        {
            _context = context;
        }

        public ResultDto<UserOrderDto> Execute(long userId)
        {
            var user = _context.Users
                .Where(p => p.Id == userId)
                .Select(p => new UserOrderDto
                {
                    UserId = p.Id,
                    FullName = p.FullName,
                    Email = p.Email
                })
                .FirstOrDefault();

            if (user == null)
            {
                return new ResultDto<UserOrderDto>
                {
                    IsSuccess = false,
                    Message = "کاربر یافت نشد",
                    Data = null
                };
            }

            return new ResultDto<UserOrderDto>
            {
                IsSuccess = true,
                Data = user
            };
        }
    }
    public class UserOrderDto
    {
        public long UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }
}
