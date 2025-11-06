using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web_store.Common.Dto;
using Web_Store.Application.Services.Products.Commands.AddNewCategory;
using Web_Store.Application.Services.Products.Commands.DeleteCategory;
using Web_Store.Application.Services.Products.Commands.EditCategory;
using Web_Store.Application.Services.Products.Queries.GetCategories;

namespace EndPoint.Site.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize("Operator")]
    public class CategoriesController : Controller
    {
        private readonly IGetCategoriesService _getCategoriesService;
        private readonly IAddNewCategoryService _addNewCategoryService;
        private readonly IEditCategoryService _editCategoryService;
        private readonly IDeleteCategoryService _deleteCategoryService;

        public CategoriesController(
            IGetCategoriesService getCategoriesService,
            IAddNewCategoryService addNewCategoryService,
            IEditCategoryService editCategoryService,
            IDeleteCategoryService deleteCategoryService)
        {
            _getCategoriesService = getCategoriesService;
            _addNewCategoryService = addNewCategoryService;
            _editCategoryService = editCategoryService;
            _deleteCategoryService = deleteCategoryService;
        }

        public IActionResult Index(long? parentId)
        {
            
            var categoriesResult = _getCategoriesService.Execute(parentId);
            if (!categoriesResult.IsSuccess)
            {
                TempData["ErrorMessage"] = categoriesResult.Message;
                return View(new List<CategoriesDto>());
            }

            ViewBag.ParentId = parentId;

           
            if (parentId.HasValue)
            {
                var allCategoriesResult = _getCategoriesService.Execute(null);
                var parent = allCategoriesResult.Data?.FirstOrDefault(c => c.Id == parentId.Value);
                ViewBag.ParentCategoryName = parent?.Name;
            }

            return View(categoriesResult.Data ?? new List<CategoriesDto>());
        }

        [HttpGet]
        public IActionResult AddNewCategory(long? parentId)
        {
            ViewBag.ParentId = parentId;
            return View();
        }

        [HttpPost]
        public IActionResult AddNewCategory(long? ParentId, string Name)
        {
            var result = _addNewCategoryService.Execute(ParentId, Name);
            return Json(result);
        }

        [HttpGet]
        public IActionResult EditCategory(long id)
        {
            var allCategoriesResult = _getCategoriesService.Execute(null);
            var category = allCategoriesResult.Data?.FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        public IActionResult EditCategory(long Id, string Name)
        {
            var result = _editCategoryService.Execute(Id, Name);
            return Json(result);
        }

        [HttpPost]
        public IActionResult DeleteCategory(long id)
        {
            var result = _deleteCategoryService.Execute(id);
            return Json(result);
        }
    }
}