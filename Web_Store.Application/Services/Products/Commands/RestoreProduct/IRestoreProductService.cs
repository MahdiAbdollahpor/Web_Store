using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_store.Common.Dto;
using Web_Store.Application.Interfaces.Contexts;

namespace Web_Store.Application.Services.Products.Commands.RestoreProduct
{
    public interface IRestoreProductService
    {
        ResultDto Execute(long productId);
    }

    public class RestoreProductService : IRestoreProductService
    {
        private readonly IDataBaseContext _context;

        public RestoreProductService(IDataBaseContext context)
        {
            _context = context;
        }

        public ResultDto Execute(long productId)
        {
            try
            {
                var product = _context.Products
                    .IgnoreQueryFilters()
                    .FirstOrDefault(p => p.Id == productId && p.IsRemoved);

                if (product == null)
                {
                    return new ResultDto
                    {
                        IsSuccess = false,
                        Message = "محصول حذف شده یافت نشد"
                    };
                }

                // بازیابی محصول
                product.IsRemoved = false;
                product.RemoveTime = null;
                _context.SaveChanges();

                return new ResultDto
                {
                    IsSuccess = true,
                    Message = "محصول با موفقیت بازیابی شد"
                };
            }
            catch (Exception ex)
            {
                return new ResultDto
                {
                    IsSuccess = false,
                    Message = "خطا در بازیابی محصول: " + ex.Message
                };
            }
        }
    }
}
