using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_Store.Application.Services.Products.Queries.GetProductForAdmin;
using Web_store.Common.Dto;
using Web_Store.Application.Interfaces.Contexts;
using Microsoft.EntityFrameworkCore;
using Web_store.Common;

namespace Web_Store.Application.Services.Products.Queries.GetDeletedProducts
{
    public interface IGetDeletedProductsService
    {
        ResultDto<ProductForAdminDto> Execute(int Page = 1, int PageSize = 20);
    }

    public class GetDeletedProductsService : IGetDeletedProductsService
    {
        private readonly IDataBaseContext _context;

        public GetDeletedProductsService(IDataBaseContext context)
        {
            _context = context;
        }

        public ResultDto<ProductForAdminDto> Execute(int Page = 1, int PageSize = 20)
        {
            try
            {
                int rowCount = 0;
                var products = _context.Products
                    .IgnoreQueryFilters()
                    .Where(x => x.IsRemoved) // فقط محصولات حذف شده
                    .Include(p => p.Category)
                    .ToPaged(Page, PageSize, out rowCount)
                    .Select(p => new ProductsFormAdminList_Dto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Category = p.Category!.Name,
                        Brand = p.Brand,
                        Description = p.Description,
                        Price = p.Price,
                        Inventory = p.Inventory,
                        Displayed = p.Displayed,
                        RemoveTime = p.RemoveTime // اضافه کردن تاریخ حذف
                    }).ToList();

                return new ResultDto<ProductForAdminDto>()
                {
                    Data = new ProductForAdminDto()
                    {
                        Products = products,
                        CurrentPage = Page,
                        PageSize = PageSize,
                        RowCount = rowCount
                    },
                    IsSuccess = true,
                    Message = "",
                };
            }
            catch (Exception ex)
            {
                return new ResultDto<ProductForAdminDto>()
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "خطا در دریافت لیست محصولات حذف شده: " + ex.Message
                };
            }
        }
    }
}
