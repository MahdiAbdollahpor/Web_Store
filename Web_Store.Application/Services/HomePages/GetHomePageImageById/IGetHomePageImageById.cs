using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_store.Common.Dto;
using Web_Store.Application.Interfaces.Contexts;
using Web_Store.Domain.Entities.HomePages;

namespace Web_Store.Application.Services.HomePages.GetHomePageImageById
{
    public interface IGetHomePageImageById
    {
        ResultDto<HomePageImages> Execute(long id);
    }

    public class GetHomePageImageById : IGetHomePageImageById
    {
        private readonly IDataBaseContext _context;

        public GetHomePageImageById(IDataBaseContext context)
        {
            _context = context;
        }

        public ResultDto<HomePageImages> Execute(long id)
        {
            var image = _context.HomePageImages.Find(id);
            return new ResultDto<HomePageImages>()
            {
                Data = image,
                IsSuccess = image != null
            };
        }
    }
}
