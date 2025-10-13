using EndPoint.Site.Models;
using EndPoint.Site.Models.ViewModels.HomePages;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Web_Store.Application.Interfaces.FacadPatterns;
using Web_Store.Application.Services.Common.Queries.GetHomePageImages;
using Web_Store.Application.Services.Common.Queries.GetSlider;
using Web_Store.Application.Services.Products.Queries.GetProductForSite;

namespace EndPoint.Site.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGetSliderService _getSliderService;
        private readonly IGetHomePageImagesService _homePageImagesService;
        private readonly IProductFacad _productFacad;

        public HomeController(ILogger<HomeController> logger
            , IGetSliderService getSliderService
            , IGetHomePageImagesService homePageImagesService
            , IProductFacad productFacad)
        {
            _logger = logger;
            _getSliderService = getSliderService;
            _homePageImagesService = homePageImagesService;
            _productFacad = productFacad;
        }

        public IActionResult Index()
        {
            try
            {
                var homePage = new HomePageViewModel()
                {
                    Sliders = _getSliderService.Execute().Data ?? new List<SliderDto>(),
                    PageImages = _homePageImagesService.Execute().Data ?? new List<HomePageImagesDto>(),
                    LatestProducts = _productFacad.GetProductForSiteService.Execute(
                        Ordering.theNewest, null, 1, 5, null).Data?.Products ?? new List<ProductForSiteDto>(),
                    MostVisitedProducts = _productFacad.GetProductForSiteService.Execute(
                        Ordering.MostVisited, null, 1, 5, null).Data?.Products ?? new List<ProductForSiteDto>(),
                    Category1Products = _productFacad.GetProductForSiteService.Execute(
                        Ordering.theNewest, null, 1, 5, 1).Data?.Products ?? new List<ProductForSiteDto>(),
                    Category2Products = _productFacad.GetProductForSiteService.Execute(
                        Ordering.theNewest, null, 1, 5, 2).Data?.Products ?? new List<ProductForSiteDto>(),
                    Category3Products = _productFacad.GetProductForSiteService.Execute(
                        Ordering.theNewest, null, 1, 5, 6).Data?.Products ?? new List<ProductForSiteDto>(),
                };
                return View(homePage);
            }
            catch (Exception ex)
            {
                // مدیریت خطا - می‌توانید یک صفحه خطا نمایش دهید یا به صفحه اصلی بازگردید
                _logger.LogError(ex, "Error in HomeController Index");
                return View(new HomePageViewModel()); // نمایش صفحه با داده‌های خالی
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}