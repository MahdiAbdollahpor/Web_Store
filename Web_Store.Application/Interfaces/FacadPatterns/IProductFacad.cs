using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_Store.Application.Services.Products.Commands.AddNewCategory;
using Web_Store.Application.Services.Products.Queries.GetCategories;

namespace Web_Store.Application.Interfaces.FacadPatterns
{
    public interface IProductFacad
    {
        AddNewCategoryService AddNewCategoryService { get; }
        IGetCategoriesService GetCategoriesService { get; }
        //AddNewProductService AddNewProductService { get; }
        //IGetAllCategoriesService GetAllCategoriesService { get; }
        ///// <summary>
        ///// دریافت لیست محصولات
        ///// </summary>
        //IGetProductForAdminService GetProductForAdminService { get; }
        //IGetProductDetailForAdminService GetProductDetailForAdminService { get; }
        //IGetProductForSiteService GetProductForSiteService { get; }
        //IGetProductDetailForSiteService GetProductDetailForSiteService { get; }
    }
}
