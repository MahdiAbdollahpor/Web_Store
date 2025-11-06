using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Web_store.Common.Dto;
using Web_Store.Application.Interfaces.Contexts;
using Web_Store.Application.Services.Logs.Commands;
using Web_Store.Domain.Entities.Products;

namespace Web_Store.Application.Services.Products.Commands.EditProduct
{
    public interface IEditProductService
    {
        ResultDto Execute(RequestEditProductDto request);
    }

    public class EditProductService : IEditProductService
    {
        private readonly IDataBaseContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogService _logService;

        public EditProductService(IDataBaseContext context, IWebHostEnvironment environment, ILogService logService)
        {
            _context = context;
            _environment = environment;
            _logService = logService;
        }

        public ResultDto Execute(RequestEditProductDto request)
        {
            try
            {
                var product = _context.Products
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductFeatures)
                    .FirstOrDefault(p => p.Id == request.Id);

                if (product == null)
                {
                    return new ResultDto
                    {
                        IsSuccess = false,
                        Message = "محصول یافت نشد"
                    };
                }

                // ذخیره مقادیر قدیمی برای لاگ
                var oldValues = new
                {
                    Name = product.Name,
                    Brand = product.Brand,
                    Description = product.Description,
                    Price = product.Price,
                    Inventory = product.Inventory,
                    Displayed = product.Displayed,
                    CategoryId = product.CategoryId,
                    Features = product.ProductFeatures?.Select(f => new { f.DisplayName, f.Value }).ToList(),
                    ImagesCount = product.ProductImages?.Count ?? 0
                };

                // آپدیت اطلاعات پایه محصول
                product.Name = request.Name!;
                product.Brand = request.Brand!;
                product.Description = request.Description!;
                product.Price = request.Price;
                product.Inventory = request.Inventory;
                product.Displayed = request.Displayed;
                product.CategoryId = request.CategoryId;
                product.UpdateTime = DateTime.Now;

                // آپدیت ویژگی‌ها
                UpdateFeatures(product, request.Features!);

                // آپدیت تصاویر فقط اگر جدیدی آپلود شده باشد
                if (request.Images != null && request.Images.Count > 0 && request.Images.Any(x => x.Length > 0))
                {
                    UpdateImages(product, request.Images);
                }

                _context.SaveChanges();

                // ذخیره مقادیر جدید برای لاگ
                var newValues = new
                {
                    Name = product.Name,
                    Brand = product.Brand,
                    Description = product.Description,
                    Price = product.Price,
                    Inventory = product.Inventory,
                    Displayed = product.Displayed,
                    CategoryId = product.CategoryId,
                    Features = product.ProductFeatures?.Select(f => new { f.DisplayName, f.Value }).ToList(),
                    ImagesCount = product.ProductImages?.Count ?? 0
                };

                // ایجاد لاگ
                _logService.LogInformation(
                    "Update",
                    "Product",
                    product.Id,
                    $"محصول {request.Name} ویرایش شد",
                    JsonSerializer.Serialize(oldValues, new JsonSerializerOptions { WriteIndented = true }),
                    JsonSerializer.Serialize(newValues, new JsonSerializerOptions { WriteIndented = true })
                );

                return new ResultDto
                {
                    IsSuccess = true,
                    Message = "محصول با موفقیت ویرایش شد"
                };
            }
            catch (Exception ex)
            {
                _logService.LogError("Update", "Product", request.Id, $"خطا در ویرایش محصول: {ex.Message}");

                return new ResultDto
                {
                    IsSuccess = false,
                    Message = "خطا در ویرایش محصول: " + ex.Message
                };
            }
        }

        private void UpdateFeatures(Product product, List<EditProduct_Features> features)
        {
            // حذف ویژگی‌های قدیمی
            var existingFeatures = _context.ProductFeatures.Where(pf => pf.ProductId == product.Id);
            _context.ProductFeatures.RemoveRange(existingFeatures);

            // اضافه کردن ویژگی‌های جدید
            if (features != null && features.Count > 0)
            {
                foreach (var feature in features)
                {
                    if (!string.IsNullOrWhiteSpace(feature.DisplayName) && !string.IsNullOrWhiteSpace(feature.Value))
                    {
                        product.ProductFeatures!.Add(new ProductFeatures
                        {
                            DisplayName = feature.DisplayName,
                            Value = feature.Value,
                            ProductId = product.Id
                        });
                    }
                }
            }
        }

        private void UpdateImages(Product product, List<IFormFile> images)
        {
            // فقط اگر تصویری آپلود شده باشد
            var validImages = images.Where(x => x != null && x.Length > 0).ToList();
            if (!validImages.Any()) return;

            // حذف تصاویر قدیمی
            var existingImages = _context.ProductImages.Where(pi => pi.ProductId == product.Id);

            // حذف فایل‌های فیزیکی
            foreach (var image in existingImages)
            {
                var oldImagePath = Path.Combine(_environment.WebRootPath, image.Src!.TrimStart('\\', '/'));
                if (File.Exists(oldImagePath))
                {
                    try
                    {
                        File.Delete(oldImagePath);
                    }
                    catch
                    {
                        // اگر حذف فایل با خطا مواجه شد، ادامه بده
                    }
                }
            }

            _context.ProductImages.RemoveRange(existingImages);

            // اضافه کردن تصاویر جدید
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "Images", "Products");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            foreach (var image in validImages)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(fileStream);
                }

                product.ProductImages!.Add(new ProductImages
                {
                    Src = Path.Combine("Images", "Products", uniqueFileName).Replace("\\", "/"),
                    ProductId = product.Id
                });
            }
        }
    }

    public class RequestEditProductDto
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "نام محصول الزامی است")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "برند محصول الزامی است")]
        public string? Brand { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "قیمت محصول الزامی است")]
        [Range(1, int.MaxValue, ErrorMessage = "قیمت باید بیشتر از 0 باشد")]
        public int Price { get; set; }

        [Required(ErrorMessage = "موجودی محصول الزامی است")]
        [Range(0, int.MaxValue, ErrorMessage = "موجودی نمی‌تواند منفی باشد")]
        public int Inventory { get; set; }

        public bool Displayed { get; set; }

        [Required(ErrorMessage = "دسته‌بندی الزامی است")]
        public long CategoryId { get; set; }

        public List<IFormFile>? Images { get; set; }
        public List<EditProduct_Features>? Features { get; set; }
    }

    public class EditProduct_Features
    {
        public long Id { get; set; }
        public string? DisplayName { get; set; }
        public string? Value { get; set; }
    }
}