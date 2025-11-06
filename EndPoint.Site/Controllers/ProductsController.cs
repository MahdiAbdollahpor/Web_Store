using Microsoft.AspNetCore.Mvc;
using Web_Store.Application.Services.Products.Queries.GetProductForSite;
using Web_Store.Application.Services.Products.Queries.GetProductDetailForSite;

namespace EndPoint.Site.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IGetProductForSiteService _getProductForSiteService;
        private readonly IGetProductDetailForSiteService _getProductDetailForSiteService;

        public ProductsController(
            IGetProductForSiteService getProductForSiteService,
            IGetProductDetailForSiteService getProductDetailForSiteService)
        {
            _getProductForSiteService = getProductForSiteService;
            _getProductDetailForSiteService = getProductDetailForSiteService;
        }

        public IActionResult Index(Ordering ordering, string Searchkey, long? CatId = null, int page = 1, int pageSize = 20)
        {
            var result = _getProductForSiteService.Execute(ordering, Searchkey, page, pageSize, CatId);
            if (!result.IsSuccess)
            {
                // در صورت خطا، می‌توانید یک صفحه خالی یا پیام خطا نمایش دهید
                return View(new ResultProductForSiteDto
                {
                    Products = new List<ProductForSiteDto>(),
                    TotalRow = 0
                });
            }
            return View(result.Data);
        }

        public IActionResult Detail(long Id)
        {
            var result = _getProductDetailForSiteService.Execute(Id);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }
            return View(result.Data);
        }
    }
}