using Microsoft.AspNetCore.Mvc;
using Web_Store.Application.Services.Fainances.Queries.GetRequestPayForAdmin;
using Web_Store.Application.Services.Fainances.Queries.GetRequestPayDetail;
using Web_Store.Application.Services.Fainances.Commands.GenerateInvoicePdf;

namespace EndPoint.Site.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RequestPayController : Controller
    {
        private readonly IGetRequestPayForAdminService _getRequestPayForAdminService;
        private readonly IGetRequestPayDetailService _getRequestPayDetailService;
        private readonly IGenerateInvoicePdfService _generateInvoicePdfService;

        public RequestPayController(
            IGetRequestPayForAdminService getRequestPayForAdminService,
            IGetRequestPayDetailService getRequestPayDetailService,
            IGenerateInvoicePdfService generateInvoicePdfService)
        {
            _getRequestPayForAdminService = getRequestPayForAdminService;
            _getRequestPayDetailService = getRequestPayDetailService;
            _generateInvoicePdfService = generateInvoicePdfService;
        }

        
        public IActionResult Index()
        {
            return View(_getRequestPayForAdminService.Execute().Data);
        }

        [HttpGet]
        [Route("Details")]
        public IActionResult Details(long id)
        {
            var result = _getRequestPayDetailService.Execute(id);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            return View(result.Data);
        }

        [HttpGet]
        [Route("DownloadPdf")]
        public IActionResult DownloadPdf(long id)
        {
            var result = _generateInvoicePdfService.Execute(id);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            var invoiceData = _getRequestPayDetailService.Execute(id);
            var fileName = $"Invoice_{invoiceData.Data!.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            return File(result.Data!, "application/pdf", fileName);
        }
    }
}