using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Web_store.Common.Dto;
using Web_Store.Application.Interfaces.Contexts;
using Web_Store.Application.Services.Logs.Commands;
using Web_Store.Domain.Entities.Products;

namespace Web_Store.Application.Services.Products.Commands.PermanentDeleteProduct
{
    public interface IPermanentDeleteProductService
    {
        ResultDto Execute(long productId);
    }


    public class PermanentDeleteProductService : IPermanentDeleteProductService
    {
        private readonly IDataBaseContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogService _logService;

        public PermanentDeleteProductService(IDataBaseContext context, IWebHostEnvironment environment, ILogService logService)
        {
            _context = context;
            _environment = environment;
            _logService = logService;
        }

        public ResultDto Execute(long productId)
        {
            try
            {
                // استفاده از IgnoreQueryFilters برای پیدا کردن محصول حتی اگر IsRemoved = true باشد
                var product = _context.Products
                    .IgnoreQueryFilters()
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductFeatures)
                    .FirstOrDefault(p => p.Id == productId);

                if (product == null)
                {
                    return new ResultDto
                    {
                        IsSuccess = false,
                        Message = "محصول یافت نشد"
                    };
                }

                // حذف تصاویر از سرور
                DeleteProductImages(product);

                // حذف ویژگی‌های محصول
                var features = _context.ProductFeatures.Where(pf => pf.ProductId == productId);
                _context.ProductFeatures.RemoveRange(features);

                // حذف تصاویر محصول از دیتابیس
                var images = _context.ProductImages.Where(pi => pi.ProductId == productId);
                _context.ProductImages.RemoveRange(images);

                // حذف محصول از دیتابیس
                _context.Products.Remove(product);
                _context.SaveChanges();

                _logService.LogWarning("PermanentDelete", "Product", productId, $"محصول به طور دائمی حذف شد");

                return new ResultDto
                {
                    IsSuccess = true,
                    Message = "محصول به طور کامل و دائمی حذف شد"
                };
            }
            catch (Exception ex)
            {
                return new ResultDto
                {
                    IsSuccess = false,
                    Message = "خطا در حذف کامل محصول: " + ex.Message
                };
            }
        }

        private void DeleteProductImages(Product product)
        {
            if (product.ProductImages != null && product.ProductImages.Any())
            {
                foreach (var image in product.ProductImages)
                {
                    // حذف فایل فیزیکی از سرور
                    var imagePath = Path.Combine(_environment.WebRootPath, image.Src.TrimStart('\\', '/'));
                    if (File.Exists(imagePath))
                    {
                        try
                        {
                            File.Delete(imagePath);
                        }
                        catch (Exception ex)
                        {
                            // اگر حذف فایل با خطا مواجه شد، لاگ کنید اما ادامه دهید
                            System.Diagnostics.Debug.WriteLine($"Error deleting image {imagePath}: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}
