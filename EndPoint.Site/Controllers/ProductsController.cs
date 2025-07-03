using Microsoft.AspNetCore.Mvc;
using System.Drawing.Printing;
using Web_Store.Application.Interfaces.FacadPatterns;

namespace EndPoint.Site.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductFacad _productFacad;

        public ProductsController(IProductFacad productFacad)
        {
            _productFacad = productFacad;
        }
        public IActionResult Index(string Searchkey, long? CatId = null, int page = 1, int pageSize = 20)
        {
            return View(_productFacad.GetProductForSiteService.Execute(Searchkey, page, pageSize, CatId).Data);
        }


        public IActionResult Detail(long Id)
        {
            return View(_productFacad.GetProductDetailForSiteService.Execute(Id).Data);
        }
    }
}
