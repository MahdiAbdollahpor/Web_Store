using Web_Store.Application.Services.Common.Queries.GetHomePageImages;
using Web_Store.Application.Services.Common.Queries.GetSlider;
using Web_Store.Application.Services.Products.Queries.GetProductForSite;

namespace EndPoint.Site.Models.ViewModels.HomePages
{
    public class HomePageViewModel
    {
        public List<SliderDto> Sliders { get; set; }
        public List<HomePageImagesDto> PageImages { get; set; }

        // محصولات جدید
        public List<ProductForSiteDto> LatestProducts { get; set; }

        // محصولات پربازدید
        public List<ProductForSiteDto> MostVisitedProducts { get; set; }

        // محصولات از 3 دسته‌بندی انتخابی
        public List<ProductForSiteDto> Category1Products { get; set; }
        public List<ProductForSiteDto> Category2Products { get; set; }
        public List<ProductForSiteDto> Category3Products { get; set; }
    }
}
