using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_store.Common.Dto;
using Web_Store.Application.Interfaces.Contexts;
using Web_Store.Application.Services.Products.Commands.DeleteCategory;

namespace Web_Store.Application.Services.Products.Commands.EditCategory
{
    public interface IEditCategoryService
    {
        ResultDto Execute(long Id, string Name);
    }

    public class EditCategoryService : IEditCategoryService
    {
        private readonly IDataBaseContext _context;

        public EditCategoryService(IDataBaseContext context)
        {
            _context = context;
        }

        public ResultDto Execute(long Id, string Name)
        {
            if (string.IsNullOrEmpty(Name))
            {
                return new ResultDto()
                {
                    IsSuccess = false,
                    Message = "نام دسته بندی را وارد نمایید",
                };
            }

            var category = _context.Categories.Find(Id);
            if (category == null)
            {
                return new ResultDto()
                {
                    IsSuccess = false,
                    Message = "دسته بندی یافت نشد",
                };
            }

            category.Name = Name;
            _context.SaveChanges();

            return new ResultDto()
            {
                IsSuccess = true,
                Message = "دسته بندی با موفقیت ویرایش شد",
            };
        }
    }
}

