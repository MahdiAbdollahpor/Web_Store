using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Web_Store.Application.Services.Fainances.Commands.GenerateInvoicePdf;
using Web_Store.Application.Services.Fainances.Queries.GetRequestPayDetail;
using Web_Store.Application.Services.Orders.Queries.GetOrderDetailsService;
using Web_Store.Application.Services.Orders.Queries.GetUserOrders;
using Web_Store.Application.Services.Users.Queries.GetUserInfoForUserPanel;

namespace EndPoint.Site.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IGetUserInfoForUserPanel _getUserInfo;
        private readonly IGetUserOrdersService _getUserOrders;
        private readonly IGetRequestPayDetailService _getRequestPayDetail;
        private readonly IGenerateInvoicePdfService _generateInvoicePdf;
        private readonly IGetOrderDetailsService _getOrderDetails;

        public DashboardController(
            IGetUserInfoForUserPanel getUserInfo,
            IGetUserOrdersService getUserOrders,
            IGetRequestPayDetailService getRequestPayDetail,
            IGenerateInvoicePdfService generateInvoicePdf,
            IGetOrderDetailsService getOrderDetails)
        {
            _getUserInfo = getUserInfo;
            _getUserOrders = getUserOrders;
            _getRequestPayDetail = getRequestPayDetail;
            _generateInvoicePdf = generateInvoicePdf;
            _getOrderDetails = getOrderDetails;
        }

        public IActionResult Index()
        {
            var userId = GetCurrentUserId();
            var userInfo = _getUserInfo.Execute(userId);
            var userOrders = _getUserOrders.Execute(userId);

            var model = new DashboardViewModel
            {
                UserInfo = userInfo,
                Orders = userOrders.Data?.Take(5).ToList()! // نمایش 5 سفارش آخر
            };

            return View(model);
        }

        public IActionResult Orders()
        {
            var userId = GetCurrentUserId();
            var result = _getUserOrders.Execute(userId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = "خطا در دریافت اطلاعات سفارشات";
                return RedirectToAction("Index");
            }

            return View(result.Data);
        }

        public IActionResult OrderDetails(long orderId)
        {
            var userId = GetCurrentUserId();
            var result = _getOrderDetails.Execute(orderId, userId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Orders");
            }

            return View(result.Data);
        }

        public IActionResult RequestPayDetail(long requestPayId)
        {
            var result = _getRequestPayDetail.Execute(requestPayId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Orders");
            }

            // بررسی اینکه این فاکتور متعلق به کاربر جاری است
            var userId = GetCurrentUserId();
            if (result.Data!.UserId != userId)
            {
                TempData["ErrorMessage"] = "دسترسی غیر مجاز";
                return RedirectToAction("Orders");
            }

            return View(result.Data);
        }

        public IActionResult DownloadInvoice(long requestPayId)
        {
            var result = _generateInvoicePdf.Execute(requestPayId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Orders");
            }

            // بررسی مالکیت فاکتور
            var requestPayDetail = _getRequestPayDetail.Execute(requestPayId);
            if (requestPayDetail.IsSuccess && requestPayDetail.Data!.UserId != GetCurrentUserId())
            {
                TempData["ErrorMessage"] = "دسترسی غیر مجاز";
                return RedirectToAction("Orders");
            }

            return File(result.Data!, "application/pdf", $"Invoice_{requestPayId}.pdf");
        }

        public IActionResult Profile()
        {
            var userId = GetCurrentUserId();
            var userInfo = _getUserInfo.Execute(userId);
            return View(userInfo);
        }

        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out long userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID not found");
        }
    }

    public class DashboardViewModel
    {
        public UserInfoDtoForDashbord? UserInfo { get; set; }
        public List<GetUserOrdersDto>? Orders { get; set; }
    }
}
