using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web_Store.Application.Interfaces.FacadPatterns;
using Web_Store.Application.Services.Logs.Commands;
using Web_Store.Application.Services.Logs.FacadPattern;
using Web_Store.Application.Services.Logs.Queries;

namespace EndPoint.Site.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class LogsController : Controller
    {
        private readonly ILogFacad _logFacad;
        private readonly IWebHostEnvironment _environment;

        public LogsController(ILogFacad logFacad, IWebHostEnvironment environment)
        {
            _logFacad = logFacad;
            _environment = environment;
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
                var result = _logFacad.GetLogDetailsService.Execute(id);

                if (!result.IsSuccess || result.Data == null)
                {
                    TempData["ErrorMessage"] = result.Message ?? "لاگ مورد نظر یافت نشد";
                    return RedirectToAction("Index");
                }

                return View(result.Data);
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
                if (days <= 0 || days > 365)
                {
                    return Json(new
                    {
                        success = false,
                        message = "تعداد روز باید بین 1 تا 365 باشد"
                    });
                }

                var result = _logFacad.ClearLogsService.Execute(days);

                return Json(new
                {
                    success = result.IsSuccess,
                    message = result.Message
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

        [HttpPost]
        public async Task<IActionResult> ClearLogsAsync(int days = 30)
        {
            try
            {
                if (days <= 0 || days > 365)
                {
                    return Json(new
                    {
                        success = false,
                        message = "تعداد روز باید بین 1 تا 365 باشد"
                    });
                }

                var result = await _logFacad.ClearLogsService.ExecuteAsync(days);

                return Json(new
                {
                    success = result.IsSuccess,
                    message = result.Message
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

        [HttpPost]
        public IActionResult Export([FromBody] ExportLogsRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "درخواست نامعتبر است"
                    });
                }

                var result = _logFacad.ExportLogsService.Execute(request);

                if (result.IsSuccess)
                {
                    return Json(new
                    {
                        success = true,
                        message = result.Message,
                        data = result.Data
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = result.Message
                    });
                }
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

        [HttpGet]
        [Route("Admin/Logs/DownloadExport/{fileName}")]
        public IActionResult DownloadExport(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, "exports", "logs", fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("فایل مورد نظر یافت نشد");
                }

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var contentType = GetContentType(fileName);

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "خطا در دانلود فایل: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return extension switch
            {
                ".json" => "application/json",
                ".csv" => "text/csv",
                ".xml" => "application/xml",
                _ => "application/octet-stream"
            };
        }
    }
}