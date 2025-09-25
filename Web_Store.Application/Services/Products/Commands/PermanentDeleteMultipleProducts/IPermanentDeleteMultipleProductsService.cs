using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_store.Common.Dto;
using Web_Store.Application.Interfaces.Contexts;
using Web_Store.Domain.Entities.Products;

namespace Web_Store.Application.Services.Products.Commands.PermanentDeleteMultipleProducts
{
    public interface IPermanentDeleteMultipleProductsService
    {
        ResultDto Execute(long[] productIds);
    }

    public class PermanentDeleteMultipleProductsService : IPermanentDeleteMultipleProductsService
    {
        private readonly IDataBaseContext _context;
        private readonly IWebHostEnvironment _environment;

        public PermanentDeleteMultipleProductsService(IDataBaseContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public ResultDto Execute(long[] productIds)
        {
            try
            {
                if (productIds == null || !productIds.Any())
                {
                    return new ResultDto
                    {
                        IsSuccess = false,
                        Message = "هیچ محصولی برای حذف انتخاب نشده است"
                    };
                }

                // پیدا کردن محصولات با IgnoreQueryFilters
                var products = _context.Products
                    .IgnoreQueryFilters()
                    .Include(p => p.ProductImages)
                    .Where(p => productIds.Contains(p.Id))
                    .ToList();

                if (!products.Any())
                {
                    return new ResultDto
                    {
                        IsSuccess = false,
                        Message = "هیچ محصولی یافت نشد"
                    };
                }

                var results = new System.Collections.Generic.List<ResultDto>();

                foreach (var product in products)
                {
                    try
                    {
                        // حذف تصاویر از سرور
                        DeleteProductImages(product);

                        // حذف ویژگی‌های محصول
                        var features = _context.ProductFeatures.Where(pf => pf.ProductId == product.Id);
                        _context.ProductFeatures.RemoveRange(features);

                        // حذف تصاویر محصول از دیتابیس
                        var images = _context.ProductImages.Where(pi => pi.ProductId == product.Id);
                        _context.ProductImages.RemoveRange(images);

                        // حذف محصول از دیتابیس
                        _context.Products.Remove(product);

                        results.Add(new ResultDto { IsSuccess = true, Message = $"محصول {product.Name} حذف شد" });
                    }
                    catch (Exception ex)
                    {
                        results.Add(new ResultDto { IsSuccess = false, Message = $"خطا در حذف محصول {product.Name}: {ex.Message}" });
                    }
                }

                _context.SaveChanges();

                var successCount = results.Count(r => r.IsSuccess);
                var failedCount = results.Count(r => !r.IsSuccess);

                string message;
                if (failedCount == 0)
                {
                    message = $"{successCount} محصول با موفقیت حذف دائمی شد.";
                }
                else if (successCount == 0)
                {
                    message = "هیچ محصولی حذف نشد. لطفاً دوباره تلاش کنید.";
                }
                else
                {
                    message = $"{successCount} محصول حذف شد، {failedCount} محصول حذف نشد.";
                }

                return new ResultDto
                {
                    IsSuccess = successCount > 0,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                return new ResultDto
                {
                    IsSuccess = false,
                    Message = "خطا در حذف کامل محصولات: " + ex.Message
                };
            }
        }

        private void DeleteProductImages(Product product)
        {
            if (product.ProductImages != null && product.ProductImages.Any())
            {
                foreach (var image in product.ProductImages)
                {
                    var imagePath = Path.Combine(_environment.WebRootPath, image.Src.TrimStart('\\', '/'));
                    if (File.Exists(imagePath))
                    {
                        try
                        {
                            File.Delete(imagePath);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error deleting image {imagePath}: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}
