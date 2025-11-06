using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_store.Common.Dto;
using Web_Store.Application.Interfaces.Contexts;

namespace Web_Store.Application.Services.Products.Commands.DeleteCategory
{
    public interface IDeleteCategoryService
    {
        ResultDto Execute(long Id);
    }

    public class DeleteCategoryService : IDeleteCategoryService
    {
        private readonly IDataBaseContext _context;

        public DeleteCategoryService(IDataBaseContext context)
        {
            _context = context;
        }

        public ResultDto Execute(long Id)
        {
            var category = _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefault(c => c.Id == Id);

            if (category == null)
                return new ResultDto { IsSuccess = false, Message = "دسته‌بندی یافت نشد." };

            if (category.SubCategories!.Any())
                return new ResultDto { IsSuccess = false, Message = "ابتدا زیرمجموعه‌های این دسته را حذف کنید." };

            // اگر محصولی به این دسته متصل است، نمی‌توان حذف کرد (بسته به نیاز شما)
            var hasProducts = _context.Products.Any(p => p.CategoryId == Id);
            if (hasProducts)
                return new ResultDto { IsSuccess = false, Message = "این دسته دارای محصول است و قابل حذف نیست." };

            _context.Categories.Remove(category);
            _context.SaveChanges();

            return new ResultDto { IsSuccess = true, Message = "دسته‌بندی با موفقیت حذف شد." };
        }
    }
}
