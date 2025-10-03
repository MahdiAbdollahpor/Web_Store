using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_store.Common.Dto;
using Web_Store.Application.Interfaces.Contexts;
using Web_Store.Domain.Entities.HomePages;

namespace Web_Store.Application.Services.HomePages.GetAllHomePageImages
{
    public interface IGetAllHomePageImages
    {
        ResultDto<List<HomePageImages>> Execute();
    }

    public class GetAllHomePageImages : IGetAllHomePageImages
    {
        private readonly IDataBaseContext _context;

        public GetAllHomePageImages(IDataBaseContext context)
        {
            _context = context;
        }

        public ResultDto<List<HomePageImages>> Execute()
        {
            var images = _context.HomePageImages
                .OrderByDescending(x => x.Id)
                .ToList();

            return new ResultDto<List<HomePageImages>>()
            {
                Data = images,
                IsSuccess = true
            };
        }
    }
}
