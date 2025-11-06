using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_Store.Application.Interfaces.Contexts;
using Web_store.Common.Dto;
using Microsoft.EntityFrameworkCore;
using Web_store.Common;

namespace Web_Store.Application.Services.Products.Queries.GetProductForSite
{
    public class GetProductForSiteService : IGetProductForSiteService
    {

        private readonly IDataBaseContext _context;
        public GetProductForSiteService(IDataBaseContext context)
        {
            _context = context;
        }
        public ResultDto<ResultProductForSiteDto> Execute(Ordering ordering, string SearchKey, int Page, int pageSize, long? CatId)
        {
            try
            {
                int totalRow = 0;
                var productQuery = _context.Products
                    .Where(p => !p.IsRemoved)
                    .Include(p => p.ProductImages)
                    .AsQueryable();

                if (CatId != null)
                {
                    productQuery = productQuery.Where(p =>
                        p.CategoryId == CatId ||
                        (p.Category != null && p.Category.ParentCategoryId == CatId)
                    ).AsQueryable();
                }

                if (!string.IsNullOrWhiteSpace(SearchKey))
                {
                    productQuery = productQuery.Where(p =>
                        p.Name!.Contains(SearchKey) ||
                        p.Brand!.Contains(SearchKey)
                    ).AsQueryable();
                }

                // مرتب‌سازی
                switch (ordering)
                {
                    case Ordering.NotOrder:
                        productQuery = productQuery.OrderByDescending(p => p.Id);
                        break;
                    case Ordering.MostVisited:
                        productQuery = productQuery.OrderByDescending(p => p.ViewCount);
                        break;
                    case Ordering.theNewest:
                        productQuery = productQuery.OrderByDescending(p => p.Id);
                        break;
                    case Ordering.Cheapest:
                        productQuery = productQuery.OrderBy(p => p.Price);
                        break;
                    case Ordering.theMostExpensive:
                        productQuery = productQuery.OrderByDescending(p => p.Price);
                        break;
                    default:
                        productQuery = productQuery.OrderByDescending(p => p.Id);
                        break;
                }

                var products = productQuery.ToPaged(Page, pageSize, out totalRow);

                Random rd = new Random();
                return new ResultDto<ResultProductForSiteDto>
                {
                    Data = new ResultProductForSiteDto
                    {
                        TotalRow = totalRow,
                        Products = products.Select(p => new ProductForSiteDto
                        {
                            Id = p.Id,
                            Star = rd.Next(1, 5),
                            Title = p.Name,
                            // اصلاح این خطا - بررسی null برای ProductImages
                            ImageSrc = p.ProductImages?.FirstOrDefault()?.Src ?? "/images/default-product.png",
                            Price = p.Price
                        }).ToList(),
                    },
                    IsSuccess = true,
                };
            }
            catch (Exception)
            {
                // لاگ کردن خطا
                return new ResultDto<ResultProductForSiteDto>
                {
                    IsSuccess = false,
                    Message = "خطا در دریافت اطلاعات محصولات"
                };
            }
        }
    }
}
