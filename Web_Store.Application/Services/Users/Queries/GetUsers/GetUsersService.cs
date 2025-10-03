using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_Store.Application.Interfaces.Contexts;
using Web_store.Common;

namespace Web_Store.Application.Services.Users.Queries.GetUsers
{
    public class GetUsersService : IGetUsersService
    {
        private readonly IDataBaseContext _context;
        public GetUsersService(IDataBaseContext context)
        {
            _context = context;
        }


        public ReslutGetUserDto Execute(RequestGetUserDto request)
        {
            var users = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchKey))
            {
                users = users.Where(p => p.FullName.Contains(request.SearchKey) || p.Email.Contains(request.SearchKey));
            }

            int rowsCount = users.Count();
            var usersList = users.Skip((request.Page - 1) * request.PageSize)
                                .Take(request.PageSize)
                                .Select(p => new GetUsersDto
                                {
                                    Email = p.Email,
                                    FullName = p.FullName,
                                    Id = p.Id,
                                    IsActive = p.IsActive
                                }).ToList();

            return new ReslutGetUserDto
            {
                Rows = rowsCount,
                Users = usersList,
                CurrentPage = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
