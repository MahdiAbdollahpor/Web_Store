using EndPoint.Site.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web_Store.Application.Services.Carts;
using Web_Store.Application.Services.Fainances.Commands.AddRequestPay;

namespace EndPoint.Site.Controllers
{
    [Authorize]
    public class PayController : Controller
    {
        private readonly IAddRequestPayService _addRequestPayService;
        private readonly ICartService _cartService;
        private readonly CookiesManeger _cookiesManeger;

        public PayController(IAddRequestPayService addRequestPayService, ICartService cartService, CookiesManeger cookiesManeger)
        {
            _addRequestPayService = addRequestPayService;
            _cartService = cartService;
            _cookiesManeger = new CookiesManeger();
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
