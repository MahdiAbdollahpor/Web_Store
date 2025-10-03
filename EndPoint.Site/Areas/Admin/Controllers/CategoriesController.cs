using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web_Store.Application.Interfaces.FacadPatterns;

namespace EndPoint.Site.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize("Operator")]
    public class CategoriesController : Controller
    {
        private readonly IProductFacad _productFacad;

        public CategoriesController(IProductFacad productFacad)
        {
            _productFacad = productFacad;
        }

        public IActionResult Index(long? parentId)
        {
            ViewBag.ParentId = parentId;
            var parentCategory = parentId.HasValue ?
                _productFacad.GetCategoriesService.Execute(null).Data
                    .FirstOrDefault(c => c.Id == parentId)?.Name : null;
            ViewBag.ParentCategoryName = parentCategory;

            return View(_productFacad.GetCategoriesService.Execute(parentId).Data);
        }

        [HttpGet]
        public IActionResult AddNewCategory(long? parentId)
        {
            ViewBag.parentId = parentId;
            return View();
        }

        [HttpPost]
        public IActionResult AddNewCategory(long? ParentId, string Name)
        {
            var result = _productFacad.AddNewCategoryService.Execute(ParentId, Name);
            return Json(result);
        }

        [HttpGet]
        public IActionResult EditCategory(long id)
        {
            var category = _productFacad.GetCategoriesService.Execute(null).Data
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        public IActionResult EditCategory(long Id, string Name)
        {
            var result = _productFacad.EditCategoryService.Execute(Id, Name);
            return Json(result);
        }

        [HttpPost]
        public IActionResult DeleteCategory(long id)
        {
            var result = _productFacad.DeleteCategoryService.Execute(id);
            return Json(result);
        }
    }
}