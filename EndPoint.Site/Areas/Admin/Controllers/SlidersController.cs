using Microsoft.AspNetCore.Mvc;
using Web_Store.Application.Services.HomePages.AddNewSlider;
using Web_Store.Application.Services.Sliders;

namespace EndPoint.Site.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SlidersController : Controller
    {
        private readonly ISliderService _sliderService;

        public SlidersController(ISliderService sliderService)
        {
            _sliderService = sliderService;
        }

        public IActionResult Index()
        {
            var result = _sliderService.GetAllSliders();
            return View(result.Data);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [Route("AddSlider")]
        public IActionResult Add(IFormFile file, string link)
        {
            var result = _sliderService.AddSlider(new RequestAddSliderDto
            {
                File = file,
                Link = link
            });

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return Redirect("/admin/sliders/");
            }

            TempData["ErrorMessage"] = result.Message;
            return View();
        }

        public IActionResult Edit(long id)
        {
            var result = _sliderService.GetSliderById(id);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = "اسلایدر یافت نشد";
                return Redirect("/admin/sliders/");
            }

            return View(result.Data);
        }

        [HttpPost]
        [Route("EditSlider")]
        public IActionResult Edit(long id, IFormFile file, string link)
        {
            var result = _sliderService.EditSlider(new RequestEditSliderDto
            {
                Id = id,
                File = file,
                Link = link
            });

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return Redirect("/admin/sliders/");
            }

            TempData["ErrorMessage"] = result.Message;
            return View();
        }

        [HttpPost]
        [Route("DeleteSlider")]
        public IActionResult Delete(long id)
        {
            var result = _sliderService.DeleteSlider(id);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return Redirect("/admin/sliders/");
        }
    }
}
