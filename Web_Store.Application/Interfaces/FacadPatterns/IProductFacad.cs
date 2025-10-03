using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_Store.Application.Services.Products.Commands.AddNewCategory;
using Web_Store.Application.Services.Products.Commands.AddNewProduct;
using Web_Store.Application.Services.Products.Commands.DeleteCategory;
using Web_Store.Application.Services.Products.Commands.DeleteProduct;
using Web_Store.Application.Services.Products.Commands.EditCategory;
using Web_Store.Application.Services.Products.Commands.EditProduct;
using Web_Store.Application.Services.Products.Commands.PermanentDeleteMultipleProducts;
using Web_Store.Application.Services.Products.Commands.PermanentDeleteProduct;
using Web_Store.Application.Services.Products.Commands.RestoreProduct;
using Web_Store.Application.Services.Products.Queries.GetAllCategories;
using Web_Store.Application.Services.Products.Queries.GetCategories;
using Web_Store.Application.Services.Products.Queries.GetDeletedProducts;
using Web_Store.Application.Services.Products.Queries.GetProductDetailForAdmin;
using Web_Store.Application.Services.Products.Queries.GetProductDetailForSite;
using Web_Store.Application.Services.Products.Queries.GetProductForAdmin;
using Web_Store.Application.Services.Products.Queries.GetProductForSite;

namespace Web_Store.Application.Interfaces.FacadPatterns
{
    public interface IProductFacad
    {
        AddNewCategoryService AddNewCategoryService { get; }
        IGetCategoriesService GetCategoriesService { get; }
        IEditCategoryService EditCategoryService { get; }
        IDeleteCategoryService DeleteCategoryService { get; }
        AddNewProductService AddNewProductService { get; }
        IEditProductService EditProductService { get; }
        IDeleteProductService DeleteProductService { get; }
        IRestoreProductService RestoreProductService { get; }
        IPermanentDeleteProductService PermanentDeleteProductService { get; }
        IPermanentDeleteMultipleProductsService PermanentDeleteMultipleProductsService { get; }
        IGetAllCategoriesService GetAllCategoriesService { get; }
        /// <summary>
        /// دریافت لیست محصولات
        /// </summary>
        IGetProductForAdminService GetProductForAdminService { get; }
        IGetProductDetailForAdminService GetProductDetailForAdminService { get; }
        IGetProductForSiteService GetProductForSiteService { get; }
        IGetProductDetailForSiteService GetProductDetailForSiteService { get; }
        IGetDeletedProductsService GetDeletedProductsService { get; }
    }
}
