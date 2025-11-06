using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web_store.Common.Dto;
using Web_Store.Application.Services.Products.Commands.AddNewProduct;
using Web_Store.Application.Services.Products.Commands.DeleteProduct;
using Web_Store.Application.Services.Products.Commands.EditProduct;
using Web_Store.Application.Services.Products.Commands.PermanentDeleteMultipleProducts;
using Web_Store.Application.Services.Products.Commands.PermanentDeleteProduct;
using Web_Store.Application.Services.Products.Commands.RestoreProduct;
using Web_Store.Application.Services.Products.Queries.GetAllCategories;
using Web_Store.Application.Services.Products.Queries.GetDeletedProducts;
using Web_Store.Application.Services.Products.Queries.GetProductDetailForAdmin;
using Web_Store.Application.Services.Products.Queries.GetProductForAdmin;

namespace EndPoint.Site.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly IGetProductForAdminService _getProductForAdminService;
        private readonly IGetProductDetailForAdminService _getProductDetailForAdminService;
        private readonly IGetAllCategoriesService _getAllCategoriesService;
        private readonly IAddNewProductService _addNewProductService;
        private readonly IEditProductService _editProductService;
        private readonly IDeleteProductService _deleteProductService;
        private readonly IGetDeletedProductsService _getDeletedProductsService;
        private readonly IRestoreProductService _restoreProductService;
        private readonly IPermanentDeleteProductService _permanentDeleteProductService;
        private readonly IPermanentDeleteMultipleProductsService _permanentDeleteMultipleProductsService;

        public ProductsController(
            IGetProductForAdminService getProductForAdminService,
            IGetProductDetailForAdminService getProductDetailForAdminService,
            IGetAllCategoriesService getAllCategoriesService,
            IAddNewProductService addNewProductService,
            IEditProductService editProductService,
            IDeleteProductService deleteProductService,
            IGetDeletedProductsService getDeletedProductsService,
            IRestoreProductService restoreProductService,
            IPermanentDeleteProductService permanentDeleteProductService,
            IPermanentDeleteMultipleProductsService permanentDeleteMultipleProductsService)
        {
            _getProductForAdminService = getProductForAdminService;
            _getProductDetailForAdminService = getProductDetailForAdminService;
            _getAllCategoriesService = getAllCategoriesService;
            _addNewProductService = addNewProductService;
            _editProductService = editProductService;
            _deleteProductService = deleteProductService;
            _getDeletedProductsService = getDeletedProductsService;
            _restoreProductService = restoreProductService;
            _permanentDeleteProductService = permanentDeleteProductService;
            _permanentDeleteMultipleProductsService = permanentDeleteMultipleProductsService;
        }

        public IActionResult Index(int Page = 1, int PageSize = 20)
        {
            var result = _getProductForAdminService.Execute(Page, PageSize);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            return View(result.Data ?? new ProductForAdminDto());
        }

        public IActionResult Detail(long Id)
        {
            var result = _getProductDetailForAdminService.Execute(Id);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }
            return View(result.Data);
        }

        [HttpGet]
        public IActionResult AddNewProduct()
        {
            var categoriesResult = _getAllCategoriesService.Execute();
            ViewBag.Categories = new SelectList(categoriesResult.Data ?? new List<AllCategoriesDto>(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public IActionResult AddNewProduct(RequestAddNewProductDto request, List<AddNewProduct_Features> Features)
        {
            var images = Request.Form.Files?.Where(f => f.Length > 0).ToList() ?? new List<IFormFile>();
            request.Images = images;
            request.Features = Features;

            var result = _addNewProductService.Execute(request);
            return Json(result);
        }

        [HttpGet]
        public IActionResult EditProduct(long id)
        {
            var productResult = _getProductDetailForAdminService.Execute(id);
            if (!productResult.IsSuccess || productResult.Data == null)
            {
                return NotFound();
            }

            var categoriesResult = _getAllCategoriesService.Execute();
            ViewBag.Categories = new SelectList(categoriesResult.Data ?? new List<AllCategoriesDto>(), "Id", "Name");
            ViewBag.CurrentImages = productResult.Data.Images;

            var editModel = new RequestEditProductDto
            {
                Id = productResult.Data.Id,
                Name = productResult.Data.Name,
                Brand = productResult.Data.Brand,
                Description = productResult.Data.Description,
                Price = productResult.Data.Price,
                Inventory = productResult.Data.Inventory,
                Displayed = productResult.Data.Displayed,
                CategoryId = GetCategoryIdFromName(productResult.Data.Category!),
                Features = productResult.Data.Features?.Select(f => new EditProduct_Features
                {
                    Id = f.Id,
                    DisplayName = f.DisplayName,
                    Value = f.Value
                }).ToList() ?? new List<EditProduct_Features>()
            };

            return View(editModel);
        }

        [HttpPost]
        public IActionResult EditProduct(RequestEditProductDto request, List<EditProduct_Features> Features)
        {
            var images = Request.Form.Files?.Where(f => f.Length > 0).ToList() ?? new List<IFormFile>();
            request.Images = images;
            request.Features = Features;

            var result = _editProductService.Execute(request);
            return Json(new
            {
                IsSuccess = result.IsSuccess,
                Message = result.Message,
                RedirectUrl = result.IsSuccess ? Url.Action("Index", "Products", new { area = "Admin" }) : ""
            });
        }

        private long GetCategoryIdFromName(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
                return 0;

            var categoriesResult = _getAllCategoriesService.Execute();
            var categories = categoriesResult.Data ?? new List<AllCategoriesDto>();
            var category = categories.FirstOrDefault(c => categoryName.Contains(c.Name ?? ""));
            return category?.Id ?? 0;
        }

        [HttpPost]
        public IActionResult DeleteProduct(long id)
        {
            var result = _deleteProductService.Execute(id);
            return Json(result);
        }

        [HttpPost]
        public IActionResult DeleteMultipleProducts(List<long> productIds)
        {
            if (productIds == null || !productIds.Any())
            {
                return Json(new ResultDto { IsSuccess = false, Message = "هیچ محصولی انتخاب نشده است." });
            }

            var successCount = 0;
            foreach (var id in productIds)
            {
                var result = _deleteProductService.Execute(id);
                if (result.IsSuccess) successCount++;
            }

            string message;
            if (successCount == productIds.Count)
                message = $"{successCount} محصول با موفقیت حذف شدند.";
            else if (successCount == 0)
                message = "هیچ محصولی حذف نشد.";
            else
                message = $"{successCount} از {productIds.Count} محصول حذف شدند.";

            return Json(new ResultDto
            {
                IsSuccess = successCount > 0,
                Message = message
            });
        }

        [HttpGet]
        public IActionResult DeletedProducts(int Page = 1, int PageSize = 20)
        {
            var result = _getDeletedProductsService.Execute(Page, PageSize);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return View(new ProductForAdminDto
                {
                    Products = new List<ProductsFormAdminList_Dto>(),
                    CurrentPage = Page,
                    PageSize = PageSize,
                    RowCount = 0
                });
            }
            return View(result.Data);
        }

        [HttpPost]
        public IActionResult RestoreProduct(long id)
        {
            var result = _restoreProductService.Execute(id);
            return Json(result);
        }

        [HttpPost]
        public IActionResult RestoreMultipleProducts(List<long> productIds)
        {
            if (productIds == null || !productIds.Any())
            {
                return Json(new ResultDto { IsSuccess = false, Message = "هیچ محصولی برای بازیابی انتخاب نشده است." });
            }

            var successCount = 0;
            foreach (var id in productIds)
            {
                var result = _restoreProductService.Execute(id);
                if (result.IsSuccess) successCount++;
            }

            string message;
            if (successCount == productIds.Count)
                message = $"{successCount} محصول با موفقیت بازیابی شدند.";
            else if (successCount == 0)
                message = "هیچ محصولی بازیابی نشد.";
            else
                message = $"{successCount} از {productIds.Count} محصول بازیابی شدند.";

            return Json(new ResultDto
            {
                IsSuccess = successCount > 0,
                Message = message
            });
        }

        [HttpPost]
        public IActionResult PermanentDeleteProduct(long id)
        {
            var result = _permanentDeleteProductService.Execute(id);
            return Json(result);
        }

        [HttpPost]
        public IActionResult PermanentDeleteMultipleProducts(long[] productIds)
        {
            if (productIds == null || productIds.Length == 0)
            {
                return Json(new ResultDto { IsSuccess = false, Message = "هیچ محصولی برای حذف دائمی انتخاب نشده است." });
            }

            var result = _permanentDeleteMultipleProductsService.Execute(productIds);
            return Json(result);
        }
    }
}