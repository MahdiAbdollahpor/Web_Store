using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Web_store.Common.Roles;
using Web_Store.Application.Interfaces.Contexts;
using Web_Store.Application.Services.Carts;
using Web_Store.Application.Services.Common.Queries.GetCategory;
using Web_Store.Application.Services.Common.Queries.GetHomePageImages;
using Web_Store.Application.Services.Common.Queries.GetMenuItem;
using Web_Store.Application.Services.Common.Queries.GetSlider;
using Web_Store.Application.Services.Fainances.Commands.AddRequestPay;
using Web_Store.Application.Services.Fainances.Commands.GenerateInvoicePdf;
using Web_Store.Application.Services.Fainances.Commands.ZarinPalService;
using Web_Store.Application.Services.Fainances.Queries.GetRequestPayDetail;
using Web_Store.Application.Services.Fainances.Queries.GetRequestPayForAdmin;
using Web_Store.Application.Services.Fainances.Queries.GetRequestPayService;
using Web_Store.Application.Services.HomePages.AddHomePageImages;
using Web_Store.Application.Services.HomePages.AddNewSlider;
using Web_Store.Application.Services.HomePages.DeleteHomePageImages;
using Web_Store.Application.Services.HomePages.EditHomePageImages;
using Web_Store.Application.Services.HomePages.GetAllHomePageImages;
using Web_Store.Application.Services.HomePages.GetHomePageImageById;
using Web_Store.Application.Services.Orders.Commands.AddNewOrder;
using Web_Store.Application.Services.Orders.Commands.ChangeOrderStateService;
using Web_Store.Application.Services.Orders.Queries.GetOrdersForAdmin;
using Web_Store.Application.Services.Orders.Queries.GetUserOrders;
using Web_Store.Application.Services.Orders.Queries.IGetOrderInvoiceServiceForAdmin;
using Web_Store.Application.Services.Orders.Queries.IGetUserServiceForAdmin;
using Web_Store.Application.Services.Products.Commands.EditProduct;
using Web_Store.Application.Services.Sliders;
using Web_Store.Application.Services.Users.Commands.EditUser;
using Web_Store.Application.Services.Users.Commands.RemoveUser;
using Web_Store.Application.Services.Users.Commands.RgegisterUser;
using Web_Store.Application.Services.Users.Commands.UserLogin;
using Web_Store.Application.Services.Users.Commands.UserSatusChange;
using Web_Store.Application.Services.Users.Queries.GetRoles;
using Web_Store.Application.Services.Users.Queries.GetUsers;
using Web_Store.Persistence.Contexts;
using Web_Store.Application.Services.Users.Queries.GetUserInfoForUserPanel;
using Web_Store.Application.Services.Orders.Queries.GetOrderDetailsService;
using Web_Store.Application.Services.Logs.Commands;
using Web_Store.Application.Services.Logs.Queries;
using Web_Store.Application.Services.Products.Commands.AddNewCategory;
using Web_Store.Application.Services.Products.Commands.AddNewProduct;
using Web_Store.Application.Services.Products.Commands.DeleteCategory;
using Web_Store.Application.Services.Products.Commands.DeleteProduct;
using Web_Store.Application.Services.Products.Commands.EditCategory;
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


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(UserRoles.Admin, policy => policy.RequireRole(UserRoles.Admin));
    options.AddPolicy(UserRoles.Customer, policy => policy.RequireRole(UserRoles.Customer));
    options.AddPolicy(UserRoles.Operator, policy => policy.RequireRole(UserRoles.Operator));
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(options =>
{
    options.LoginPath = new PathString("/Authentication/Signin");
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5.0);
    options.AccessDeniedPath = new PathString("/Authentication/Signin");
});

builder.Services.AddDbContext<DataBaseContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("MyConnection")));
// دیتابیس 
builder.Services.AddScoped<IDataBaseContext, DataBaseContext>();
// کاربر 
builder.Services.AddScoped<IGetUsersService, GetUsersService>();
builder.Services.AddScoped<IGetRolesService, GetRolesService>();
builder.Services.AddScoped<IRegisterUserService, RegisterUserService>();
builder.Services.AddScoped<IRemoveUserService, RemoveUserService>();
builder.Services.AddScoped<IUserLoginService, UserLoginService>();
builder.Services.AddScoped<IUserSatusChangeService, UserSatusChangeService>();
builder.Services.AddScoped<IEditUserService, EditUserService>();
builder.Services.AddScoped<IGetUserService, GetUserService>();
builder.Services.AddScoped<IGetUserInfoForUserPanel, GetUserInfoForUserPanel>();
// منو 
builder.Services.AddScoped<IGetMenuItemService, GetMenuItemService>();
//دسته بندی ها 
builder.Services.AddScoped<IGetCategoryService, GetCategoryService>();
// اسلایدر ها و بنر ها 
builder.Services.AddScoped<IAddNewSliderService, AddNewSliderService>();
builder.Services.AddScoped<IAddHomePageImagesService, AddHomePageImagesService>();
builder.Services.AddScoped<IGetSliderService, GetSliderService>();
builder.Services.AddScoped<IGetHomePageImagesService, GetHomePageImagesService>();
builder.Services.AddScoped<IDeleteHomePageImages, DeleteHomePageImages>();
builder.Services.AddScoped<IEditHomePageImages, EditHomePageImages>();
builder.Services.AddScoped<IGetAllHomePageImages, GetAllHomePageImages>();
builder.Services.AddScoped<IGetHomePageImageById, GetHomePageImageById>();
builder.Services.AddScoped<ISliderService, SliderService>();
// سبد خرید 
builder.Services.AddScoped<ICartService, CartService>();
// پرداخت ها 
builder.Services.AddHttpClient<ZarinPalService>();
builder.Services.AddScoped<IAddRequestPayService, AddRequestPayService>();
builder.Services.AddScoped<IGetRequestPayService, GetRequestPayService>();
builder.Services.AddScoped<IGetRequestPayForAdminService, GetRequestPayForAdminService>();
builder.Services.AddScoped<IGetRequestPayDetailService, GetRequestPayDetailService>();
builder.Services.AddScoped<IGenerateInvoicePdfService, GenerateInvoicePdfService>();
// سفارش ها 
builder.Services.AddScoped<IAddNewOrderService, AddNewOrderService>();
builder.Services.AddScoped<IGetUserOrdersService, GetUserOrdersService>();
builder.Services.AddScoped<IGetOrdersForAdminService, GetOrdersForAdminService>();
builder.Services.AddScoped<IGetOrderInvoiceService, GetOrderInvoiceService>();
builder.Services.AddScoped<IChangeOrderStateService, ChangeOrderStateService>();
builder.Services.AddScoped<IGetOrderDetailsService, GetOrderDetailsService>();
// محصولات
builder.Services.AddScoped<IAddNewCategoryService, AddNewCategoryService>();
builder.Services.AddScoped<IGetCategoriesService, GetCategoriesService>();
builder.Services.AddScoped<IEditCategoryService, EditCategoryService>();
builder.Services.AddScoped<IDeleteCategoryService, DeleteCategoryService>();
builder.Services.AddScoped<IAddNewProductService, AddNewProductService>();
builder.Services.AddScoped<IEditProductService, EditProductService>();
builder.Services.AddScoped<IDeleteProductService, DeleteProductService>();
builder.Services.AddScoped<IRestoreProductService, RestoreProductService>();
builder.Services.AddScoped<IPermanentDeleteProductService, PermanentDeleteProductService>();
builder.Services.AddScoped<IPermanentDeleteMultipleProductsService, PermanentDeleteMultipleProductsService>();
builder.Services.AddScoped<IGetAllCategoriesService, GetAllCategoriesService>();
builder.Services.AddScoped<IGetProductForAdminService, GetProductForAdminService>();
builder.Services.AddScoped<IGetProductDetailForAdminService, GetProductDetailForAdminService>();
builder.Services.AddScoped<IGetProductForSiteService, GetProductForSiteService>();
builder.Services.AddScoped<IGetProductDetailForSiteService, GetProductDetailForSiteService>();
builder.Services.AddScoped<IGetDeletedProductsService, GetDeletedProductsService>();

// لاگ‌ها
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IClearLogsService, ClearLogsService>();
builder.Services.AddScoped<IGetLogsService, GetLogsService>();
builder.Services.AddScoped<IGetLogDetailsService, GetLogDetailsService>();
builder.Services.AddScoped<IExportLogsService, ExportLogsService>();
builder.Services.AddHttpContextAccessor();




builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.Run();
