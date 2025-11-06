using EndPoint.Site.Utilities;
using Microsoft.AspNetCore.Mvc;
using Web_Store.Application.Services.Carts;
using Web_Store.Application.Services.Fainances.Commands.AddRequestPay;
using Web_Store.Application.Services.Fainances.Commands.ZarinPalService;
using Web_Store.Application.Services.Fainances.Queries.GetRequestPayService;
using Web_Store.Application.Services.Orders.Commands.AddNewOrder;
using Web_Store.Domain.Entities.Users;

public class PayController : Controller
{
    private readonly ZarinPalService _zarinPalService;
    private readonly IAddRequestPayService _addRequestPayService;
    private readonly ICartService _cartService;
    private readonly CookiesManeger _cookiesManeger;
    private readonly IGetRequestPayService _getRequestPayService;
    private readonly IAddNewOrderService _addNewOrderService;

    public PayController(
        ZarinPalService zarinPalService,
        IAddRequestPayService addRequestPayService,
        ICartService cartService,
        IGetRequestPayService getRequestPayService,
        IAddNewOrderService addNewOrderService)
    {
        _zarinPalService = zarinPalService;
        _addRequestPayService = addRequestPayService;
        _cartService = cartService;
        _cookiesManeger = new CookiesManeger();
        _getRequestPayService = getRequestPayService;
        _addNewOrderService = addNewOrderService;
    }

    public async Task<IActionResult> Index()
    {
        long? UserId = ClaimUtility.GetUserId(User);
        var cart = _cartService.GetMyCart(_cookiesManeger.GetBrowserId(HttpContext), UserId);

        if (cart.Data!.SumAmount > 1000) // حداقل amount
        {
            var requestPay = _addRequestPayService.Execute(cart.Data.SumAmount, UserId!.Value);

            try
            {
                var request = new ZarinPalRequest
                {
                    MerchantId = "6c729f63-9724-4349-9e69-786f9aee2658",
                    Amount = cart.Data.SumAmount,
                    CallbackUrl = $"https://localhost:44328/Pay/Verify?guid={requestPay.Data!.guid}",
                    Description = "پرداخت فاکتور شماره:" + requestPay.Data.RequestPayId,
                    Email = requestPay.Data.Email,
                    Mobile = "09121112222"
                };

                var result = await _zarinPalService.RequestPayment(request);
                return Redirect($"https://sandbox.zarinpal.com/pg/StartPay/{result.data!.authority}");
            }
            catch (Exception ex)
            {
                // مدیریت خطا
                return RedirectToAction("PaymentError", "Home", new { message = ex.Message });
            }
        }
        else
        {
            return RedirectToAction("Index", "Cart");
        }
    }

    public async Task<IActionResult> Verify(Guid guid, string authority, string status)
    {
        var requestPay = _getRequestPayService.Execute(guid);

        try
        {
            var verifyRequest = new ZarinPalVerifyRequest
            {
                MerchantId = "6c729f63-9724-4349-9e69-786f9aee2658",
                Amount = requestPay.Data!.Amount,
                Authority = authority
            };

            var result = await _zarinPalService.VerifyPayment(verifyRequest);

            if (result.data!.code == 100)
            {
                // پرداخت موفق
                long? UserId = ClaimUtility.GetUserId(User);
                var cart = _cartService.GetMyCart(_cookiesManeger.GetBrowserId(HttpContext), UserId);

                _addNewOrderService.Execute(new RequestAddNewOrderSericeDto
                {
                    CartId = cart.Data!.CartId,
                    UserId = UserId!.Value,
                    RequestPayId = requestPay.Data.Id
                });

                return RedirectToAction("Index", "Orders");
            }
            else
            {
                // پرداخت ناموفق
                return RedirectToAction("PaymentFailed", "Home");
            }
        }
        catch (Exception ex)
        {
            // مدیریت خطا
            return RedirectToAction("PaymentError", "Home", new { message = ex.Message });
        }
    }
}