using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Web_store.Common.Dto;
using Web_Store.Application.Interfaces.Contexts;
using Web_Store.Application.Services.Logs.Commands;
using Web_Store.Domain.Entities.Products;

namespace Web_Store.Application.Services.Products.Commands.DeleteProduct
{
    public interface IDeleteProductService
    {
        ResultDto Execute(long productId);
    }


    public class DeleteProductService : IDeleteProductService
    {
        private readonly IDataBaseContext _context;
        private readonly ILogService _logService;


        public DeleteProductService(IDataBaseContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public ResultDto Execute(long productId)
        {
            try
            {
                var product = _context.Products
                    .FirstOrDefault(p => p.Id == productId && !p.IsRemoved);

                if (product == null)
                {
                    return new ResultDto
                    {
                        IsSuccess = false,
                        Message = "محصول یافت نشد"
                    };
                }

                // Soft Delete - فقط علامت گذاری به عنوان حذف شده
                product.IsRemoved = true;
                product.RemoveTime = DateTime.Now;
                _context.SaveChanges();

                _logService.LogInformation("SoftDelete", "Product", productId, $"محصول به سطل زباله منتقل شد");

                return new ResultDto
                {
                    IsSuccess = true,
                    Message = "محصول با موفقیت حذف شد"
                };
            }
            catch (Exception ex)
            {
                return new ResultDto
                {
                    IsSuccess = false,
                    Message = "خطا در حذف محصول: " + ex.Message
                };
            }
        }
    }

   
    }
