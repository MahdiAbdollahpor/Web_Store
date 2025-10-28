using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web_Store.Application.Services.Orders.Commands.ChangeOrderStateService;
using Web_Store.Application.Services.Orders.Queries.GetOrdersForAdmin;
using Web_Store.Application.Services.Orders.Queries.IGetOrderInvoiceServiceForAdmin;
using Web_Store.Application.Services.Orders.Queries.IGetUserServiceForAdmin;
using Web_Store.Domain.Entities.Orders;

namespace EndPoint.Site.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = "Admin,Operator")]
    public class OrdersController : Controller
    {
        private readonly IGetOrdersForAdminService _getOrdersForAdminService;
        private readonly IGetUserService _getUserService;
        private readonly IGetOrderInvoiceService _getOrderInvoiceService;
        private readonly IChangeOrderStateService _changeOrderStateService;

        public OrdersController(
            IGetOrdersForAdminService getOrdersForAdminService,
            IGetUserService getUserService,
            IGetOrderInvoiceService getOrderInvoiceService,
            IChangeOrderStateService changeOrderStateService)
        {
            _getOrdersForAdminService = getOrdersForAdminService;
            _getUserService = getUserService;
            _getOrderInvoiceService = getOrderInvoiceService;
            _changeOrderStateService = changeOrderStateService;
        }

        [HttpGet]
        public IActionResult Index(OrderState? orderState)
        {
            var state = orderState ?? OrderState.Processing; // حالت پیش‌فرض
            return View(_getOrdersForAdminService.Execute(state).Data);
        }

        [HttpGet]
        [Route("UserDetails")]
        public IActionResult UserDetails(long userId)
        {
            var result = _getUserService.Execute(userId);
            if (!result.IsSuccess)
            {
                return NotFound();
            }
            return View(result.Data);
        }

        [HttpGet]
        [Route("Invoice")]
        public IActionResult Invoice(long orderId)
        {
            var result = _getOrderInvoiceService.Execute(orderId);
            if (!result.IsSuccess)
            {
                return NotFound();
            }
            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ChangeOrderState")]
        public IActionResult ChangeOrderState(long orderId, OrderState newOrderState)
        {
            var request = new RequestChangeOrderStateDto
            {
                OrderId = orderId,
                NewOrderState = newOrderState
            };

            var result = _changeOrderStateService.Execute(request);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction("Index", new { orderState = newOrderState });
        }
    }
}