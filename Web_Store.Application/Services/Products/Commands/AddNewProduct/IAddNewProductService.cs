using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Web_Store.Application.Interfaces.Contexts;
using Web_store.Common.Dto;
using Web_Store.Application.Services.Logs;
using Web_Store.Domain.Entities.Products;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Web_Store.Application.Services.Logs.Commands;

namespace Web_Store.Application.Services.Products.Commands.AddNewProduct
{
    public interface IAddNewProductService
    {
        ResultDto Execute(RequestAddNewProductDto request);
    }

    public class AddNewProductService : IAddNewProductService
    {
        private readonly IDataBaseContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogService _logService;

        public AddNewProductService(IDataBaseContext context, IWebHostEnvironment environment, ILogService logService)
        {
            _context = context;
            _environment = environment;
            _logService = logService;
        }

        public ResultDto Execute(RequestAddNewProductDto request)
        {
            try
            {
                // بررسی وجود دسته‌بندی
                var category = _context.Categories.Find(request.CategoryId);
                if (category == null)
                {
                    return new ResultDto
                    {
                        IsSuccess = false,
                        Message = "دسته‌بندی یافت نشد",
                    };
                }

                // ایجاد محصول جدید
                Product product = new Product()
                {
                    Brand = request.Brand,
                    Description = request.Description,
                    Name = request.Name,
                    Price = request.Price,
                    Inventory = request.Inventory,
                    Category = category,
                    Displayed = request.Displayed,
                };
                _context.Products.Add(product);

                // آپلود و ذخیره تصاویر
                List<ProductImages> productImages = new List<ProductImages>();
                foreach (var item in request.Images)
                {
                    var uploadedResult = UploadFile(item);
                    if (uploadedResult.Status)
                    {
                        productImages.Add(new ProductImages
                        {
                            Product = product,
                            Src = uploadedResult.FileNameAddress,
                        });
                    }
                }

                if (productImages.Any())
                {
                    _context.ProductImages.AddRange(productImages);
                }

                // افزودن ویژگی‌های محصول
                List<ProductFeatures> productFeatures = new List<ProductFeatures>();
                foreach (var item in request.Features)
                {
                    productFeatures.Add(new ProductFeatures
                    {
                        DisplayName = item.DisplayName,
                        Value = item.Value,
                        Product = product,
                    });
                }
                _context.ProductFeatures.AddRange(productFeatures);

                _context.SaveChanges();

                // ایجاد لاگ برای افزودن محصول
                var productData = new
                {
                    Name = product.Name,
                    Brand = product.Brand,
                    Price = product.Price,
                    Inventory = product.Inventory,
                    Category = category.Name,
                    ImagesCount = productImages.Count,
                    FeaturesCount = productFeatures.Count
                };

                _logService.LogInformation(
                    "Create",
                    "Product",
                    product.Id,
                    $"محصول جدید {request.Name} افزوده شد",
                    null, // برای ایجاد، oldValues null است
                    JsonSerializer.Serialize(productData, new JsonSerializerOptions { WriteIndented = true })
                );

                return new ResultDto
                {
                    IsSuccess = true,
                    Message = "محصول با موفقیت به محصولات فروشگاه اضافه شد",
                };
            }
            catch (Exception ex)
            {
                _logService.LogError("Create", "Product", 0, $"خطا در افزودن محصول: {ex.Message}");

                return new ResultDto
                {
                    IsSuccess = false,
                    Message = "خطا رخ داد: " + ex.Message,
                };
            }
        }

        private UploadDto UploadFile(IFormFile file)
        {
            if (file != null && file.Length > 0 && _environment != null)
            {
                string folder = @"images/ProductImages/";
                var uploadsRootFolder = Path.Combine(_environment.WebRootPath, folder);

                // ایجاد پوشه اگر وجود ندارد
                if (!Directory.Exists(uploadsRootFolder))
                {
                    Directory.CreateDirectory(uploadsRootFolder);
                }

                // تولید نام فایل منحصر به فرد
                string fileName = $"{DateTime.Now.Ticks}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadsRootFolder, fileName);

                // ذخیره فایل
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                return new UploadDto()
                {
                    FileNameAddress = Path.Combine(folder, fileName).Replace("\\", "/"),
                    Status = true,
                };
            }

            return new UploadDto()
            {
                Status = false,
                FileNameAddress = "",
            };
        }
    }

    public class UploadDto
    {
        public long Id { get; set; }
        public bool Status { get; set; }
        public string FileNameAddress { get; set; }
    }

    public class RequestAddNewProductDto
    {
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public int Inventory { get; set; }
        public long CategoryId { get; set; }
        public bool Displayed { get; set; }
        public List<IFormFile> Images { get; set; }
        public List<AddNewProduct_Features> Features { get; set; }
    }

    public class AddNewProduct_Features
    {
        public string DisplayName { get; set; }
        public string Value { get; set; }
    }
}