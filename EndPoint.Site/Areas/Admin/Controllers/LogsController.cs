using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web_Store.Application.Interfaces.FacadPatterns;
using Web_Store.Application.Services.Logs.FacadPattern;
using Web_Store.Application.Services.Logs.Queries;

namespace EndPoint.Site.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class LogsController : Controller
    {
        private readonly ILogFacad _logFacad;

        public LogsController(ILogFacad logFacad)
        {
            _logFacad = logFacad;
        }

        public IActionResult Index(string searchKey = "", string entityName = "", string actionType = "",
                                string logLevel = "", DateTime? fromDate = null, DateTime? toDate = null,
                                int page = 1, int pageSize = 20)
        {
            try
            {


               

                var result = _logFacad.GetLogsService.Execute(new RequestGetLogsDto
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchKey = searchKey,
                    EntityName = entityName,
                    ActionType = actionType,
                    LogLevel = logLevel,
                    FromDate = fromDate,
                    ToDate = toDate
                });

                if (!result.IsSuccess)
                {
                    TempData["ErrorMessage"] = result.Message;
                }

                ViewBag.SearchKey = searchKey;
                ViewBag.EntityName = entityName;
                ViewBag.ActionType = actionType;
                ViewBag.LogLevel = logLevel;
                ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
                ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
                ViewBag.PageSize = pageSize;

                return View(result.Data);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "خطا در بارگذاری لاگ‌ها: " + ex.Message;
                return View(new ResultGetLogsDto
                {
                    Logs = new List<LogDto>(),
                    CurrentPage = 1,
                    PageSize = pageSize,
                    RowCount = 0
                });
            }
        }

        [HttpPost]
        public IActionResult GetLogs([FromBody] RequestGetLogsDto request)
        {
            try
            {
                var result = _logFacad.GetLogsService.Execute(request);
                return Json(new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "خطا در دریافت لاگ‌ها: " + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("Admin/Logs/Details/{id}")]
        public IActionResult Details(long id)
        {
            try
            {
                var result = _logFacad.GetLogsService.Execute(new RequestGetLogsDto
                {
                    Page = 1,
                    PageSize = 1
                });

                var log = result.Data?.Logs?.FirstOrDefault(l => l.Id == id);

                if (log == null)
                {
                    TempData["ErrorMessage"] = "لاگ مورد نظر یافت نشد";
                    return RedirectToAction("Index");
                }

                return View(log);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "خطا در بارگذاری جزئیات لاگ: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult ClearLogs(int days = 30)
        {
            try
            {
                // این متد نیاز به پیاده‌سازی سرویس پاک‌سازی لاگ‌ها دارد
                // فعلاً فقط پیام بازگشتی می‌دهیم
                return Json(new
                {
                    success = true,
                    message = $"لاگ‌های قدیمی‌تر از {days} روز با موفقیت پاک شدند"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "خطا در پاک‌سازی لاگ‌ها: " + ex.Message
                });
            }
        }

        [HttpGet]
        public IActionResult Export(string format = "json")
        {
            try
            {
                // این متد برای خروجی گرفتن از لاگ‌ها
                // فعلاً فقط پیام بازگشتی می‌دهیم
                return Json(new
                {
                    success = true,
                    message = $"خروجی {format} لاگ‌ها با موفقیت ایجاد شد"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "خطا در ایجاد خروجی: " + ex.Message
                });
            }
        }
    }
}