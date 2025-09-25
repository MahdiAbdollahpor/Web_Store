
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web_Store.Application.Interfaces.Contexts;
using Web_Store.Application.Interfaces.FacadPatterns;
using Web_Store.Application.Services.Products.Commands.AddNewCategory;
using Web_Store.Application.Services.Products.Commands.AddNewProduct;
using Web_Store.Application.Services.Products.Commands.DeleteProduct;
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

namespace Web_Store.Application.Services.Products.FacadPattern
{
    public class ProductFacad : IProductFacad
    {
        private readonly IDataBaseContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductFacad(IDataBaseContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        private AddNewCategoryService _addNewCategory;
        public AddNewCategoryService AddNewCategoryService
        {
            get
            {
                return _addNewCategory = _addNewCategory ?? new AddNewCategoryService(_context);
            }
        }


        private IGetCategoriesService _getCategoriesService;
        public IGetCategoriesService GetCategoriesService
        {
            get
            {
                return _getCategoriesService = _getCategoriesService ?? new GetCategoriesService(_context);
            }
        }

        private AddNewProductService _addNewProductService;
        public AddNewProductService AddNewProductService
        {
            get
            {
                return _addNewProductService = _addNewProductService ?? new AddNewProductService(_context, _environment);
            }
        }

        private IGetAllCategoriesService _getAllCategoriesService;
        public IGetAllCategoriesService GetAllCategoriesService
        {
            get
            {
                return _getAllCategoriesService = _getAllCategoriesService ?? new GetAllCategoriesService(_context);
            }
        }
        private IGetProductForAdminService _getProductForAdminService;
        public IGetProductForAdminService GetProductForAdminService
        {
            get
            {
                return _getProductForAdminService = _getProductForAdminService ?? new GetProductForAdminService(_context);
            }
        }


        private IGetProductDetailForAdminService _getProductDetailForAdminService;
        public IGetProductDetailForAdminService GetProductDetailForAdminService
        {
            get
            {
                return _getProductDetailForAdminService = _getProductDetailForAdminService ?? new GetProductDetailForAdminService(_context);
            }
        }

        private IGetProductDetailForSiteService _getProductDetailForSiteService;
        public IGetProductDetailForSiteService GetProductDetailForSiteService
        {
            get
            {
                return _getProductDetailForSiteService = _getProductDetailForSiteService ?? new GetProductDetailForSiteService(_context);
            }
        }

        private IGetProductForSiteService _getProductForSiteService;
        public IGetProductForSiteService GetProductForSiteService
        {
            get
            {
                return _getProductForSiteService = _getProductForSiteService ?? new GetProductForSiteService(_context);
            }
        }

        private IEditProductService _editProductService;
        public IEditProductService EditProductService
        {
            get
            {
                return _editProductService = _editProductService ?? new EditProductService(_context, _environment);
            }
        }


        private IDeleteProductService _deleteProductService;
        public IDeleteProductService DeleteProductService
        {
            get
            {
                return _deleteProductService = _deleteProductService ?? new DeleteProductService(_context);
            }
        }

        private IRestoreProductService _restoreProductService;
        public IRestoreProductService RestoreProductService
        {
            get
            {
                return _restoreProductService = _restoreProductService ?? new RestoreProductService(_context);
            }
        }

        private IGetDeletedProductsService _getDeletedProductsService;
        public IGetDeletedProductsService GetDeletedProductsService
        {
            get
            {
                return _getDeletedProductsService = _getDeletedProductsService ?? new GetDeletedProductsService(_context);
            }
        }

        private IPermanentDeleteProductService _permanentDeleteProductService;
        public IPermanentDeleteProductService PermanentDeleteProductService
        {
            get
            {
                return _permanentDeleteProductService = _permanentDeleteProductService ?? new PermanentDeleteProductService(_context, _environment);
            }
        }

        private IPermanentDeleteMultipleProductsService _permanentDeleteMultipleProductsService;
        public IPermanentDeleteMultipleProductsService PermanentDeleteMultipleProductsService
        {
            get
            {
                return _permanentDeleteMultipleProductsService = _permanentDeleteMultipleProductsService ?? new PermanentDeleteMultipleProductsService(_context, _environment);
            }
        }
    }

}

