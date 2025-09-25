using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Web_store.Common.Dto;
using Web_Store.Application.Interfaces.FacadPatterns;
using Web_Store.Application.Services.Products.Commands.AddNewProduct;
using Web_Store.Application.Services.Products.Commands.EditProduct;
using Web_Store.Application.Services.Products.Queries.GetProductDetailForAdmin;
using Web_Store.Application.Services.Products.Queries.GetProductForAdmin;
using static Web_Store.Application.Services.Products.Commands.EditProduct.EditProductService;

namespace EndPoint.Site.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {

        private readonly IProductFacad _productFacad;

        public ProductsController(IProductFacad productFacad)
        {
            _productFacad = productFacad;
        }
        public IActionResult Index(int Page = 1, int PageSize = 20)
        {
            return View(_productFacad.GetProductForAdminService.Execute(Page, PageSize).Data);
        }

        public IActionResult Detail(long Id)
        {
            return View(_productFacad.GetProductDetailForAdminService.Execute(Id).Data);
        }

        [HttpGet]
        public IActionResult AddNewProduct()
        {
            ViewBag.Categories = new SelectList(_productFacad.GetAllCategoriesService.Execute().Data, "Id", "Name");
            return View();
        }

        [HttpPost]
        public IActionResult AddNewProduct(RequestAddNewProductDto request, List<AddNewProduct_Features> Features)
        {
            List<IFormFile> images = new List<IFormFile>();
            for (int i = 0; i < Request.Form.Files.Count; i++)
            {
                var file = Request.Form.Files[i];
                images.Add(file);
            }
            request.Images = images;
            request.Features = Features;
            return Json(_productFacad.AddNewProductService.Execute(request));
        }

        [HttpGet]
        public IActionResult EditProduct(long id)
        {
            var productDetail = _productFacad.GetProductDetailForAdminService.Execute(id).Data;
            if (productDetail == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(_productFacad.GetAllCategoriesService.Execute().Data, "Id", "Name");
            ViewBag.CurrentImages = productDetail.Images;

            var editModel = new RequestEditProductDto
            {
                Id = productDetail.Id,
                Name = productDetail.Name,
                Brand = productDetail.Brand,
                Description = productDetail.Description,
                Price = productDetail.Price,
                Inventory = productDetail.Inventory,
                Displayed = productDetail.Displayed,
                CategoryId = GetCategoryIdFromName(productDetail.Category),
                Features = productDetail.Features?.Select(f => new EditProduct_Features
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
            try
            {
                // دریافت فایل‌ها - با بررسی null
                List<IFormFile> images = new List<IFormFile>();
                if (Request.Form.Files != null && Request.Form.Files.Count > 0)
                {
                    for (int i = 0; i < Request.Form.Files.Count; i++)
                    {
                        var file = Request.Form.Files[i];
                        if (file != null && file.Length > 0)
                        {
                            images.Add(file);
                        }
                    }
                }
                request.Images = images;
                request.Features = Features;

                var result = _productFacad.EditProductService.Execute(request);
                return Json(new
                {
                    IsSuccess = result.IsSuccess,
                    Message = result.Message,
                    RedirectUrl = result.IsSuccess ? "/Admin/Products/Index" : ""
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    IsSuccess = false,
                    Message = "خطا در ویرایش محصول: " + ex.Message
                });
            }
        }

        private long GetCategoryIdFromName(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
                return 0;

            var categories = _productFacad.GetAllCategoriesService.Execute().Data;
            var category = categories.FirstOrDefault(c => categoryName.Contains(c.Name));
            return category?.Id ?? categories.FirstOrDefault()?.Id ?? 0;
        }


        [HttpPost]
        public IActionResult DeleteProduct(long id)
        {
            try
            {
                var result = _productFacad.DeleteProductService.Execute(id);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    IsSuccess = false,
                    Message = "خطا در حذف محصول: " + ex.Message
                });
            }
        }

        [HttpPost]
        public IActionResult DeleteMultipleProducts(List<long> productIds)
        {
            try
            {
                if (productIds == null || !productIds.Any())
                {
                    return Json(new
                    {
                        IsSuccess = false,
                        Message = "هیچ محصولی برای حذف انتخاب نشده است"
                    });
                }

                var results = new List<ResultDto>();
                foreach (var productId in productIds)
                {
                    var result = _productFacad.DeleteProductService.Execute(productId);
                    results.Add(result);
                }

                var successCount = results.Count(r => r.IsSuccess);
                var failedCount = results.Count(r => !r.IsSuccess);

                string message;
                if (failedCount == 0)
                {
                    message = $"{successCount} محصول با موفقیت حذف شد.";
                }
                else if (successCount == 0)
                {
                    message = "هیچ محصولی حذف نشد. لطفاً دوباره تلاش کنید.";
                }
                else
                {
                    message = $"{successCount} محصول حذف شد، {failedCount} محصول حذف نشد.";
                }

                return Json(new
                {
                    IsSuccess = successCount > 0,
                    Message = message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    IsSuccess = false,
                    Message = "خطا در حذف محصولات: " + ex.Message
                });
            }
        }

       

        [HttpGet]
        public IActionResult DeletedProducts(int Page = 1, int PageSize = 20)
        {
            var result = _productFacad.GetDeletedProductsService.Execute(Page, PageSize);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return View(new ProductForAdminDto()
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
            try
            {
                var result = _productFacad.RestoreProductService.Execute(id);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    IsSuccess = false,
                    Message = "خطا در بازیابی محصول: " + ex.Message
                });
            }
        }

        [HttpPost]
        public IActionResult RestoreMultipleProducts(List<long> productIds)
        {
            try
            {
                if (productIds == null || !productIds.Any())
                {
                    return Json(new
                    {
                        IsSuccess = false,
                        Message = "هیچ محصولی برای بازیابی انتخاب نشده است"
                    });
                }

                var results = new List<ResultDto>();
                foreach (var productId in productIds)
                {
                    var result = _productFacad.RestoreProductService.Execute(productId);
                    results.Add(result);
                }

                var successCount = results.Count(r => r.IsSuccess);
                var failedCount = results.Count(r => !r.IsSuccess);

                string message;
                if (failedCount == 0)
                {
                    message = $"{successCount} محصول با موفقیت بازیابی شد.";
                }
                else if (successCount == 0)
                {
                    message = "هیچ محصولی بازیابی نشد. لطفاً دوباره تلاش کنید.";
                }
                else
                {
                    message = $"{successCount} محصول بازیابی شد، {failedCount} محصول بازیابی نشد.";
                }

                return Json(new
                {
                    IsSuccess = successCount > 0,
                    Message = message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    IsSuccess = false,
                    Message = "خطا در بازیابی محصولات: " + ex.Message
                });
            }
        }

        
        [HttpPost]
        public IActionResult PermanentDeleteProduct(long id)
        {
            try
            {
                var result = _productFacad.PermanentDeleteProductService.Execute(id);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    IsSuccess = false,
                    Message = "خطا در حذف کامل محصول: " + ex.Message
                });
            }
        }

        
        [HttpPost]
        public IActionResult PermanentDeleteMultipleProducts(long[] productIds)
        {
            try
            {
                var result = _productFacad.PermanentDeleteMultipleProductsService.Execute(productIds);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    IsSuccess = false,
                    Message = "خطا در حذف کامل محصولات: " + ex.Message
                });
            }
        }
    }
}

