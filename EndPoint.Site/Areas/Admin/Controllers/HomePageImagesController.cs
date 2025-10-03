using Microsoft.AspNetCore.Mvc;
using Web_Store.Application.Services.HomePages.AddHomePageImages;
using Web_Store.Application.Services.HomePages.DeleteHomePageImages;
using Web_Store.Application.Services.HomePages.EditHomePageImages;
using Web_Store.Application.Services.HomePages.GetAllHomePageImages;
using Web_Store.Application.Services.HomePages.GetHomePageImageById;
using Web_Store.Domain.Entities.HomePages;

namespace EndPoint.Site.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomePageImagesController : Controller
    {
        private readonly IAddHomePageImagesService _addHomePageImagesService;
        private readonly IGetHomePageImageById _getHomePageImageById;
        private readonly IEditHomePageImages _editHomePageImages;
        private readonly IDeleteHomePageImages _deleteHomePageImages;
        private readonly IGetAllHomePageImages _getAllHomePageImages;
        public HomePageImagesController(IAddHomePageImagesService addHomePageImagesService, IGetHomePageImageById getHomePageImageById, IEditHomePageImages editHomePageImages, IDeleteHomePageImages deleteHomePageImages, IGetAllHomePageImages getAllHomePageImages)
        {
            _addHomePageImagesService = addHomePageImagesService;
            _getHomePageImageById = getHomePageImageById;
            _editHomePageImages = editHomePageImages;
            _deleteHomePageImages = deleteHomePageImages;
            _getAllHomePageImages = getAllHomePageImages;
        }
        public IActionResult Index()
        {
            var result = _getAllHomePageImages.Execute();
            return View(result.Data);
        }


        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(IFormFile file, string link, ImageLocation imageLocation)
        {
            _addHomePageImagesService.Execute(new requestAddHomePageImagesDto
            {
                file = file,
                ImageLocation = imageLocation,
                Link = link,
            });
            return View();
        }

        [HttpGet]
        public IActionResult Edit(long id)
        {
            var result = _getHomePageImageById.Execute(id);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = "بنر یافت نشد";
                return Redirect("/admin/homepageimages/");
            }

            return View(result.Data);
        }

        [HttpPost]
        [Route("EditHomePage")]
        public IActionResult Edit(long id, IFormFile file, string link, ImageLocation imageLocation)
        {
            var result = _editHomePageImages.Execute(new requestEditHomePageImagesDto
            {
                Id = id,
                file = file,
                ImageLocation = imageLocation,
                Link = link,
            });

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return Redirect("/admin/homepageimages/");
            }

            TempData["ErrorMessage"] = result.Message;
            return View();
        }

        [HttpPost]
        [Route("DeleteHomePage")]
        public IActionResult Delete(long id)
        {
            var result = _deleteHomePageImages.Execute(id);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return Redirect("/admin/homepageimages/");
        }
    }
}   

